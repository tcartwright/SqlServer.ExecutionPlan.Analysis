

IF OBJECT_ID('[dbo].[xfnCheckPlanXml]') IS NOT NULL BEGIN
	DROP FUNCTION [dbo].[xfnCheckPlanXml]
END

IF OBJECT_ID('[dbo].[xfnComputeHash]') IS NOT NULL BEGIN
	DROP FUNCTION [dbo].[xfnComputeHash]
END

IF EXISTS( SELECT * FROM sys.assemblies WHERE name = N'SqlServer.ExecutionPlan.Analysis.Clr') BEGIN
	DROP ASSEMBLY [SqlServer.ExecutionPlan.Analysis.Clr]
END

IF EXISTS( SELECT * FROM sys.assemblies WHERE name = N'SqlServer.ExecutionPlan.Analysis') BEGIN
	DROP ASSEMBLY [SqlServer.ExecutionPlan.Analysis]
END

--for some reason sometimes the push names this assembly one way, sometimes the other way
IF EXISTS( SELECT * FROM sys.assemblies WHERE name = N'SqlServer.Analysis') BEGIN
	DROP ASSEMBLY [SqlServer.Analysis]
END


