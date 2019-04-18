/*
 Navicat Premium Data Transfer

 Source Server         : localhost
 Source Server Type    : SQL Server
 Source Server Version : 14001000
 Source Host           : .:1433
 Source Catalog        : xwoa
 Source Schema         : dbo

 Target Server Type    : SQL Server
 Target Server Version : 14001000
 File Encoding         : 65001

 Date: 18/04/2019 16:56:20
*/


-- ----------------------------
-- Table structure for DEP_ReceiveFile_ZhuJian
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[DEP_ReceiveFile_ZhuJian]') AND type IN ('U'))
	DROP TABLE [dbo].[DEP_ReceiveFile_ZhuJian]
GO

CREATE TABLE [dbo].[DEP_ReceiveFile_ZhuJian] (
  [Id] nvarchar(36) COLLATE Chinese_PRC_CI_AS  NOT NULL,
  [DocumentNumber] nvarchar(50) COLLATE Chinese_PRC_CI_AS  NULL,
  [DocumentTitle] nvarchar(200) COLLATE Chinese_PRC_CI_AS  NULL,
  [ClosedOrHairTime] datetime  NULL,
  [MessageId] nvarchar(36) COLLATE Chinese_PRC_CI_AS  NOT NULL,
  [WorkFlowId] nvarchar(36) COLLATE Chinese_PRC_CI_AS  NOT NULL,
  [ReceivedDocumentUnit] nvarchar(50) COLLATE Chinese_PRC_CI_AS  NULL,
  [SequenceName] nvarchar(200) COLLATE Chinese_PRC_CI_AS  NULL,
  [SequenceNumber] nvarchar(200) COLLATE Chinese_PRC_CI_AS  NULL
)
GO

ALTER TABLE [dbo].[DEP_ReceiveFile_ZhuJian] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'主键Id',
'SCHEMA', N'dbo',
'TABLE', N'DEP_ReceiveFile_ZhuJian',
'COLUMN', N'Id'
GO

EXEC sp_addextendedproperty
'MS_Description', N'文件编号',
'SCHEMA', N'dbo',
'TABLE', N'DEP_ReceiveFile_ZhuJian',
'COLUMN', N'DocumentNumber'
GO

EXEC sp_addextendedproperty
'MS_Description', N'文件标题',
'SCHEMA', N'dbo',
'TABLE', N'DEP_ReceiveFile_ZhuJian',
'COLUMN', N'DocumentTitle'
GO

EXEC sp_addextendedproperty
'MS_Description', N'收/发文日期',
'SCHEMA', N'dbo',
'TABLE', N'DEP_ReceiveFile_ZhuJian',
'COLUMN', N'ClosedOrHairTime'
GO

EXEC sp_addextendedproperty
'MS_Description', N'工作流程Id',
'SCHEMA', N'dbo',
'TABLE', N'DEP_ReceiveFile_ZhuJian',
'COLUMN', N'MessageId'
GO

EXEC sp_addextendedproperty
'MS_Description', N'工作流Id',
'SCHEMA', N'dbo',
'TABLE', N'DEP_ReceiveFile_ZhuJian',
'COLUMN', N'WorkFlowId'
GO

EXEC sp_addextendedproperty
'MS_Description', N'来文单位',
'SCHEMA', N'dbo',
'TABLE', N'DEP_ReceiveFile_ZhuJian',
'COLUMN', N'ReceivedDocumentUnit'
GO

EXEC sp_addextendedproperty
'MS_Description', N'序列名称',
'SCHEMA', N'dbo',
'TABLE', N'DEP_ReceiveFile_ZhuJian',
'COLUMN', N'SequenceName'
GO

EXEC sp_addextendedproperty
'MS_Description', N'序列编号',
'SCHEMA', N'dbo',
'TABLE', N'DEP_ReceiveFile_ZhuJian',
'COLUMN', N'SequenceNumber'
GO

