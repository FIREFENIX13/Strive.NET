SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[CreateMobileMirror]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[CreateMobileMirror]
GO




CREATE PROCEDURE CreateMobileMirror
@MobileID int,
@X int, @Y int, @Z int,
@HeadingX int, @HeadingY int, @HeadingZ int
AS
insert into RespawnPoint values (
    @MobileID,
    @X, @Y, @Z,
    @HeadingX, @HeadingY, @HeadingZ
)




GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

