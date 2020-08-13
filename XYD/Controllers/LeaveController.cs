using Appkiz.Apps.Workflow.Library;
using Appkiz.Library.Security;
using Appkiz.Library.Security.Authentication;
using FluentDate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using XYD.Common;
using XYD.Entity;
using XYD.Models;

namespace XYD.Controllers
{
    public class LeaveController : Controller
    {
        WorkflowMgr mgr = new WorkflowMgr();
        SheetMgr sheetMgr = new SheetMgr();
        OrgMgr orgMgr = new OrgMgr();

        #region 添加请假记录
        public ActionResult Add(XYD_Leave_Record model, string user, string mid)
        {
            try
            {
                using (var db = new DefaultConnection())
                {
                    var leave = db.LeaveRecord.Where(n => n.EmplID == user && n.MessageID == mid).FirstOrDefault();
                    if (leave == null)
                    {
                        model.EmplID = user;
                        model.MessageID = mid;
                        model.CreateTime = DateTime.Now;
                        model.UpdateTime = DateTime.Now;
                        model.Status = DEP_Constants.Leave_Status_Auditing;
                        if (model.Category == "补打卡")
                        {
                            model.EndDate = model.StartDate;
                        }
                        db.LeaveRecord.Add(model);
                    }
                    else
                    {
                        leave.UpdateTime = DateTime.Now;
                        leave.StartDate = model.StartDate;
                        if (model.Category == "补打卡")
                        {
                            leave.EndDate = model.StartDate;
                        }
                        else
                        {
                            leave.EndDate = model.EndDate;
                        }
                        leave.Status = DEP_Constants.Leave_Status_Auditing;
                    }
                    
                    db.SaveChanges();
                    return ResponseUtil.OK("添加请假记录成功");
                }
            }
            catch(Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion

        #region 更新申请状态
        public ActionResult UpdateLeaveStatus(string mid, string node, bool isAuditNode=false)
        {
            try
            {
                // 变量定义
                var operate = string.Empty;
                var opinion = string.Empty;
                var message = mgr.GetMessage(mid);
                var employee = orgMgr.GetEmployee(message.MessageIssuedBy);
                Doc doc = mgr.GetDocByWorksheetID(mgr.GetDocHelperIdByMessageId(mid));
                Worksheet worksheet = doc.Worksheet;
                XYD_Audit_Node auditNode = WorkflowUtil.GetAuditNode(mid, node);
                if (auditNode == null)
                {
                    return ResponseUtil.Error("没找到对应处理节点");
                }
                operate = worksheet.GetWorkcell(auditNode.Operate.Row, auditNode.Operate.Col).WorkcellValue;
                opinion = worksheet.GetWorkcell(auditNode.Opinion.Row, auditNode.Opinion.Col).WorkcellValue;

                using (var db = new DefaultConnection())
                {
                    var leave = db.LeaveRecord.Where(n => n.MessageID == mid).FirstOrDefault();
                    if (leave == null)
                    {
                        return ResponseUtil.Error("未找到对应请假");
                    }
                    else
                    {
                        if (operate == DEP_Constants.Audit_Operate_Type_Disagree)
                        {
                            leave.Status = DEP_Constants.Leave_Status_NO;
                        }
                        else if (operate == DEP_Constants.Audit_Operate_Type_Agree)
                        {
                            leave.Status = DEP_Constants.Leave_Status_YES;
                            if (!isAuditNode) // 结束节点
                            {
                                // 处理年假
                                if (leave.Category == DEP_Constants.Leave_Year_Type)
                                {
                                    var userCompanyInfo = db.UserCompanyInfo.Where(n => n.EmplID == employee.EmplID).FirstOrDefault();
                                    if (userCompanyInfo != null)
                                    {
                                        userCompanyInfo.UsedRestDays += Convert.ToInt32((leave.EndDate - leave.StartDate).TotalDays + 1);
                                    }
                                }
                                // 处理考勤记录，如果是*假、外勤、补打卡、补外勤
                                if (CalendarUtil.NeedUpdateAttence(leave))
                                {
                                    // 更新或插入考勤记录表
                                    // 如果是小时假，才更新
                                    if (leave.StartDate.Date == leave.EndDate.Date)
                                    {
                                        CalendarUtil.UpdateAttence(employee, leave);
                                    }
                                }
                            }
                        }
                    }
                    db.SaveChanges();
                }

                return ResponseUtil.OK("添加处理记录成功");
            }
            catch (Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion

        #region 年假查询
        [Authorize]
        public ActionResult QueryRest(DateTime date)
        {
            try
            {
                var employee = (User.Identity as AppkizIdentity).Employee;
                var areaKey = GetUserArea(employee.EmplID);

                var totalRestDays = 0d;
                // 获得当前起始日期
                var startYearDate = DateTime.Parse(string.Format("{0}/01/01 00:00:00", date.Year));
                var endYearDate = DateTime.Parse(string.Format("{0}/01/01 00:00:00", date.Year + 1));
                // 查询已修年假
                var db = new DefaultConnection();
                var records = db.LeaveRecord.Where(n => n.EmplID == employee.EmplID && n.StartDate >= startYearDate && n.EndDate < endYearDate && n.Category == "年假").OrderBy(n => n.CreateTime).ToList();
                var results = new List<object>();
                foreach (var record in records)
                {
                    if (record.Status == DEP_Constants.Leave_Status_YES)
                    {
                        totalRestDays += (record.EndDate - record.StartDate).TotalDays + 1;
                    }
                    var result = new
                    {
                        Avatar = string.Format("/Apps/People/Shared/do_ShowPhoto.aspx?tag=logo&emplid={0}", record.EmplID),
                        EmplName = employee.EmplName,
                        Category = record.Category,
                        StartDate = record.StartDate,
                        EndDate = record.EndDate,
                        Status = record.Status,
                        CreateTime = record.CreateTime,
                        MessageID = record.MessageID
                    };
                    results.Add(result);
                }
                // 查询剩余年假
                var leftYearHour = 0.0d;
                var leftOffTimeHour = 0.0d;
                var leftLeaveHour = 0.0d;

                // 计算总年假
                var userCompanyInfo = db.UserCompanyInfo.Where(n => n.EmplID == employee.EmplID).FirstOrDefault();
                var totalYearHour = CalendarUtil.GetUserYearHour(employee.EmplID);
                var vocationReport = CalendarUtil.CaculateVocation(employee.EmplID, startYearDate, endYearDate);

                // 已使用年假
                var usedYearHour = vocationReport.yearHour;
                // 已使用事假、调休
                var usedLeaveHour = vocationReport.leaveHour;
                // 加班时间
                var offTimeWork = vocationReport.extraHour;
                // 特殊调整
                var adjustHour = vocationReport.adjustHour;
                // 开始计算剩余
                CalendarUtil.CaculateLeftHour(totalYearHour, usedYearHour, usedLeaveHour, offTimeWork, adjustHour, ref leftYearHour, ref leftLeaveHour, ref leftOffTimeHour);

                // 已使用年假天数
                return ResponseUtil.OK(new {
                    remainDays = (int)(leftYearHour/8),
                    totalRestDays = (int)((totalYearHour-leftYearHour)/8),
                    records = results
                });
            }
            catch(Exception e)
            {
                return ResponseUtil.OK(e.Message);
            }
        }
        #endregion

        #region 计算年假
        /// <summary>
        /// * 根据实际工龄计算年假
        /// * 实际工龄不满十年，年假5天
        /// * 满十年不满二十年，年假10天
        /// * 满20年，年假15天
        /// </summary>
        /// <param name="SocialMonth"></param>
        /// <returns></returns>
        public int CaculateYearRestDays(XYD_UserCompanyInfo userCompanyInfo)
        {
            if (userCompanyInfo == null)
            {
                return 0;
            }
            if (userCompanyInfo.ManualCaculate)
            {
                return userCompanyInfo.RestDays;
            }
            else
            {
                if (userCompanyInfo.SocialInsuranceTotalMonth < 10 * 12)
                {
                    return 5;
                }
                else if (userCompanyInfo.SocialInsuranceTotalMonth < 20 * 12)
                {
                    return 10;
                }
                else
                {
                    return 15;
                }
            }
        }
        #endregion

        #region 获得用户区域
        public string GetUserArea(string emplID)
        {
            if (OrgUtil.CheckRole(emplID, DEP_Constants.Role_Name_WuXi))
            {
                return DEP_Constants.System_Config_Area_WX;
            }
            else
            {
                return DEP_Constants.System_Config_Area_SH;
            }
        }
        #endregion

        #region 添加用户出勤申请记录
        public ActionResult AddLeaveRecord2(XYD_Leave_Record model, string node, string mid)
        {
            using (var db = new DefaultConnection())
            {
                Doc doc = mgr.GetDocByWorksheetID(mgr.GetDocHelperIdByMessageId(mid));
                Worksheet worksheet = doc.Worksheet;

                var message = mgr.GetMessage(mid);
                var user = message.MessageIssuedBy;
                var employee = orgMgr.GetEmployee(user);

                model.EmplID = user;
                model.MessageID = mid;
                model.CreateTime = DateTime.Now;
                model.UpdateTime = DateTime.Now;
                if (model.Category == "补打卡")
                {
                    model.EndDate = model.StartDate;
                }

                XYD_Audit_Node auditNode = WorkflowUtil.GetAuditNode(mid, node);
                if (auditNode == null)
                {
                    return ResponseUtil.Error("没找到对应处理节点");
                }
                var operate = worksheet.GetWorkcell(auditNode.Operate.Row, auditNode.Operate.Col).WorkcellValue;

                if (operate == DEP_Constants.Audit_Operate_Type_Disagree)
                {
                    model.Status = DEP_Constants.Leave_Status_NO;
                }
                else if (operate == DEP_Constants.Audit_Operate_Type_Agree)
                {
                    model.Status = DEP_Constants.Leave_Status_YES;
                    // 处理年假
                    if (model.Category == DEP_Constants.Leave_Year_Type)
                    {
                        var userCompanyInfo = db.UserCompanyInfo.Where(n => n.EmplID == employee.EmplID).FirstOrDefault();
                        if (userCompanyInfo != null)
                        {
                            userCompanyInfo.UsedRestDays += Convert.ToInt32((model.EndDate - model.StartDate).TotalDays + 1);
                        }
                    }
                    // 处理考勤记录，如果是*假、外勤、补打卡、补外勤
                    if (CalendarUtil.NeedUpdateAttence(model))
                    {
                        // 更新或插入考勤记录表
                        // 如果是小时假，才更新
                        if (CommonUtils.SameDay(model.StartDate, model.EndDate))
                        {
                            CalendarUtil.UpdateAttence(employee, model);
                        }
                    }
                }

                db.LeaveRecord.Add(model);
                db.SaveChanges();
                return ResponseUtil.OK("添加请假记录成功");
            }
        }
        #endregion
    }
}