# 配置文件说明

## 通用配置
1. global.json
部门与映射表关系

1. workflow.json
部门与公文对应关系

2. subflow.json
配置部门中哪个公文作为子工作流


## 表单定制配置
1. mid.json
配置mapping，actions, control, details

2. mid-app-transformer.json
公文按钮权限、附件信息。

3. mid-web-transformer.json
公文移动端数据接口定义。

4. mid-subflow.json
仅当该流程作为部门子流程时会出现，配置子流程发起映射关系。

3. [TODO] notify.json
开发中，表单中节点与对应通知范围配置。

4. [TODO]
开发中，表单中节点意见排序与修改配置。