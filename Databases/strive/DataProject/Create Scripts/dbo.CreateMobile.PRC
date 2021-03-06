SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS ON 
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[CreateMobile]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[CreateMobile]
GO



CREATE PROCEDURE CreateMobile
@AreaID int, @Name nvarchar(16), @ModelID int,
@RaceID int, @Level int,
@Strength int, @Constitution int, @Cognition int, @Willpower int, @Dexterity int, 
@Hitpoints int, @GoldCarried int, @GoldBanked int,
@X int, @Y int, @Z int,
@HeadingX int, @HeadingY int, @HeadingZ int
AS
declare @PhysicalObjectID int
set NOCOUNT on
set XACT_ABORT on
begin transaction
insert into PhysicalObject
values( @AreaID, @Name, @ModelID )
set @PhysicalObjectID = SCOPE_IDENTITY()
insert into MobilePhysicalObject
values ( @PhysicalObjectID, @RaceID, @Level,
    @Strength, @Constitution, @Cognition, @Willpower, @Dexterity,
    @GoldCarried, @GoldBanked
)
insert into RespawnPoint
values ( @PhysicalObjectID, @X, @Y, @Z, @HeadingX, @HeadingY, @HeadingZ )
commit transaction


GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

