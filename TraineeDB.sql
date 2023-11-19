CREATE TABLE Trainee (
    TraineeID      INT           IDENTITY (1, 1) NOT NULL,
    TraineeCode    VARCHAR (50)  NULL,
    TraineeName    VARCHAR (100) NULL,
    CourseID INT           NULL,
    DOB        DATE          NULL,
    Gender     VARCHAR (20)  NULL,
    State      VARCHAR (15)  NULL,
    ImagePath  VARCHAR (250) NULL,
    CONSTRAINT [PK_Trainee] PRIMARY KEY CLUSTERED (TraineeID ASC)
);

CREATE TABLE TraineeTSP (
    TraineeTSPID    INT           IDENTITY (1, 1) NOT NULL,
    TraineeID       INT           NULL,
    TSPName VARCHAR (100) NULL,
    CourseID  INT           NULL,
    Round     INT           NULL
);

CREATE TABLE Course (
    CourseID INT           NOT NULL,
    Course   NVARCHAR (50) NULL,
    PRIMARY KEY CLUSTERED (CourseID ASC)
);


//SP//
CREATE PROC TraineeTSPAddOrEdit
@TraineeTSPID int,
@TraineeID int,
@TSPName varchar(100),
@CourseID int,
@Round int
AS
	--Insert
	IF @TraineeTSPID = 0
		INSERT INTO TraineeTSP(TraineeID,TSPName,CourseID,Round)
		VALUES (@TraineeID,@TSPName,@CourseID,@Round)
	--Update
	ELSE
		UPDATE TraineeTSP
		SET
			TraineeID=@TraineeID,
			TSPName=@TSPName,
			CourseID=@CourseID,
			Round=@Round
		WHERE TraineeTSPID = @TraineeTSPID

CREATE PROC TraineeTSPDelete
@TraineeTSPID int
AS
	DELETE FROM TraineeTSP
	WHERE TraineeTSPID = @TraineeTSPID


CREATE PROC TraineeAddOrEdit
@TraineeID int,
@TraineeCode varchar(50),
@TraineeName varchar(100),
@CourseID int,
@DOB date,
@Gender varchar(20),
@State varchar(15),
@ImagePath varchar(250)
AS

	--Insert
	IF @TraineeID = 0 BEGIN
		INSERT INTO Trainee(TraineeCode,TraineeName,CourseID,DOB,Gender,State,ImagePath)
		VALUES (@TraineeCode,@TraineeName,@CourseID,@DOB,@Gender,@State,@ImagePath)

		SELECT SCOPE_IDENTITY();

		END
	--Update
	ELSE BEGIN
		UPDATE Trainee
		SET
			TraineeCode=@TraineeCode,
			TraineeName=@TraineeName,
			CourseID=@CourseID,
			DOB=@DOB,
			Gender=@Gender,
			State=@State,
			ImagePath=@ImagePath
		WHERE TraineeID=@TraineeID

		SELECT @TraineeID;

		END

CREATE PROC TraineeDelete
@TraineeID int
AS
	--Master
	DELETE FROM Trainee
	WHERE TraineeID = @TraineeID
	--Details
	DELETE FROM TraineeTSP
	WHERE TraineeID = @TraineeID

CREATE PROC TraineeViewAll
AS
SELECT T.TraineeID,T.TraineeCode,T.TraineeName,C.Course,T.DOB,T.State
FROM Trainee T INNER JOIN Course C
					ON T.CourseID = C.CourseID

CREATE PROC TraineeViewByID
@TraineeID int
AS
	--Master
	SELECT *
	FROM Trainee
	WHERE TraineeID = @TraineeID
	--Details
	SELECT *
	FROM TraineeTSP
	WHERE TraineeID = @TraineeID

