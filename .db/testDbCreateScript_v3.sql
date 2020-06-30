USE [master]
GO
/****** Object:  Database [AnnotatorTest]    Script Date: 6/30/2020 4:27:07 PM ******/
CREATE DATABASE [AnnotatorTest]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'AnnotatorTest', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL15.MSSQLSERVER\MSSQL\DATA\AnnotatorTest.mdf' , SIZE = 73728KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'AnnotatorTest_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL15.MSSQLSERVER\MSSQL\DATA\AnnotatorTest_log.ldf' , SIZE = 73728KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
 WITH CATALOG_COLLATION = DATABASE_DEFAULT
GO
ALTER DATABASE [AnnotatorTest] SET COMPATIBILITY_LEVEL = 150
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [AnnotatorTest].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [AnnotatorTest] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [AnnotatorTest] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [AnnotatorTest] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [AnnotatorTest] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [AnnotatorTest] SET ARITHABORT OFF 
GO
ALTER DATABASE [AnnotatorTest] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [AnnotatorTest] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [AnnotatorTest] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [AnnotatorTest] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [AnnotatorTest] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [AnnotatorTest] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [AnnotatorTest] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [AnnotatorTest] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [AnnotatorTest] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [AnnotatorTest] SET  DISABLE_BROKER 
GO
ALTER DATABASE [AnnotatorTest] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [AnnotatorTest] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [AnnotatorTest] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [AnnotatorTest] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [AnnotatorTest] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [AnnotatorTest] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [AnnotatorTest] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [AnnotatorTest] SET RECOVERY FULL 
GO
ALTER DATABASE [AnnotatorTest] SET  MULTI_USER 
GO
ALTER DATABASE [AnnotatorTest] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [AnnotatorTest] SET DB_CHAINING OFF 
GO
ALTER DATABASE [AnnotatorTest] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [AnnotatorTest] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [AnnotatorTest] SET DELAYED_DURABILITY = DISABLED 
GO
EXEC sys.sp_db_vardecimal_storage_format N'AnnotatorTest', N'ON'
GO
ALTER DATABASE [AnnotatorTest] SET QUERY_STORE = OFF
GO
USE [AnnotatorTest]
GO
/****** Object:  FullTextCatalog [ft]    Script Date: 6/30/2020 4:27:08 PM ******/
CREATE FULLTEXT CATALOG [ft] WITH ACCENT_SENSITIVITY = ON
AS DEFAULT
GO
/****** Object:  FullTextCatalog [FTS_CATALOG]    Script Date: 6/30/2020 4:27:08 PM ******/
CREATE FULLTEXT CATALOG [FTS_CATALOG] WITH ACCENT_SENSITIVITY = OFF
GO
/****** Object:  Table [dbo].[Ontology]    Script Date: 6/30/2020 4:27:08 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Ontology](
	[ID] [bigint] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](256) NOT NULL,
	[CurrentVersion] [nvarchar](256) NOT NULL,
	[Definition] [nvarchar](1024) NOT NULL,
	[DateCreated] [datetimeoffset](7) NOT NULL,
	[UserID] [nvarchar](32) NOT NULL,
 CONSTRAINT [PK_Ontology] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [IX_Ontology_Name] UNIQUE NONCLUSTERED 
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Term]    Script Date: 6/30/2020 4:27:08 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Term](
	[ID] [bigint] IDENTITY(1,1) NOT NULL,
	[Accession] [nvarchar](128) NOT NULL,
	[OntologyID] [bigint] NOT NULL,
	[Name] [nvarchar](512) NOT NULL,
	[Definition] [nvarchar](1024) NOT NULL,
	[XRefValueType] [nvarchar](50) NULL,
	[IsObsolete] [bit] NOT NULL,
 CONSTRAINT [PK_Term] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [IX_Term_Accession] UNIQUE NONCLUSTERED 
(
	[Accession] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TermRelationship]    Script Date: 6/30/2020 4:27:08 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TermRelationship](
	[ID] [bigint] IDENTITY(1,1) NOT NULL,
	[TermID] [bigint] NOT NULL,
	[RelationshipType] [varchar](64) NOT NULL,
	[RelatedTermID] [bigint] NOT NULL,
 CONSTRAINT [PK_TermRelationship] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [IX_TermRelationship] UNIQUE NONCLUSTERED 
(
	[TermID] ASC,
	[RelatedTermID] ASC,
	[RelationshipType] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [term_Name]    Script Date: 6/30/2020 4:27:08 PM ******/
CREATE NONCLUSTERED INDEX [term_Name] ON [dbo].[Term]
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [term_Unique]    Script Date: 6/30/2020 4:27:08 PM ******/
CREATE NONCLUSTERED INDEX [term_Unique] ON [dbo].[Term]
(
	[Name] ASC,
	[Accession] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Term]  WITH CHECK ADD  CONSTRAINT [FK_Term_Ontology1] FOREIGN KEY([OntologyID])
REFERENCES [dbo].[Ontology] ([ID])
GO
ALTER TABLE [dbo].[Term] CHECK CONSTRAINT [FK_Term_Ontology1]
GO
ALTER TABLE [dbo].[TermRelationship]  WITH CHECK ADD  CONSTRAINT [FK_TermRelationship_Term] FOREIGN KEY([TermID])
REFERENCES [dbo].[Term] ([ID])
GO
ALTER TABLE [dbo].[TermRelationship] CHECK CONSTRAINT [FK_TermRelationship_Term]
GO
ALTER TABLE [dbo].[TermRelationship]  WITH CHECK ADD  CONSTRAINT [FK_TermRelationship_Term1] FOREIGN KEY([RelatedTermID])
REFERENCES [dbo].[Term] ([ID])
GO
ALTER TABLE [dbo].[TermRelationship] CHECK CONSTRAINT [FK_TermRelationship_Term1]
GO
/****** Object:  StoredProcedure [dbo].[advancedTermSearch]    Script Date: 6/30/2020 4:27:08 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[advancedTermSearch](
	@ontologyID				AS bigint, 
	@startsWith				AS nvarchar(64), 
	@mustContain			AS nvarchar(2048),
	@endsWith				AS nvarchar(64),
	@definitionMustContain	AS nvarchar(2048)
)
AS

--This is the entry point switch that executes procedures depending on the input
BEGIN
	--
	IF (@ontologyID IS NOT NULL)
		IF @mustContain = '' 
			IF @definitionMustContain = ''
				EXEC fromOntologyTermNameStartsEndsWith @ontologyId, @startsWith, @endsWith
			ELSE
				EXEC fromOntologyTermNameStartsEndsWithAndDefinitionContains @ontologyId, @startswith, @endsWith, @definitionMustContain
		ELSE
			IF @definitionMustContain = ''
				EXEC fromOntologyTermNameStartsEndsWithAndContains @ontologyId, @startsWith, @endsWith, @mustContain
			ELSE
				EXEC fromOntologyTermNameStartsEndsWithAndContainsAndDefinitionContains @ontologyId, @startsWith, @endsWith, @definitionMustContain, @mustContain
	ELSE
		IF @mustContain = '' 
			IF @definitionMustContain = ''
				EXEC termNameStartsEndsWith @startsWith, @endswith
			ELSE
				EXEC termNameStartsEndsWithAndDefinitionContains @startswith, @endsWith, @definitionMustContain
		ELSE
			IF @definitionMustContain = ''
				EXEC termNameStartsEndsWithAndContains @startsWith, @endsWith, @mustContain
			ELSE
				EXEC termNameStartsEndsWithAndContainsAndDefinitionContains @startsWith, @endsWith, @definitionMustContain, @mustContain
END;
GO
/****** Object:  StoredProcedure [dbo].[fromOntologyTermNameStartsEndsWith]    Script Date: 6/30/2020 4:27:08 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[fromOntologyTermNameStartsEndsWith](
	@ontologyId	AS bigint,
	@startsWith	AS nvarchar(64), 
	@endsWith	AS nvarchar(64)
)
AS
BEGIN
	SELECT * FROM Term WHERE
		OntologyID = @ontologyId
		AND Name LIKE (@startsWith + '%' + @endsWith) 
END;
GO
/****** Object:  StoredProcedure [dbo].[fromOntologyTermNameStartsEndsWithAndContains]    Script Date: 6/30/2020 4:27:08 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[fromOntologyTermNameStartsEndsWithAndContains](
	@ontologyId		AS bigint,
	@startsWith		AS nvarchar(64), 
	@endsWith		AS nvarchar(64),
	@mustContain	AS nvarchar(64)
)
AS
BEGIN 
	
	DECLARE @freetextSearch nvarchar(2048)
	SET @freetextSearch = 'FormsOf(INFLECTIONAL, "' + @mustContain + '")'
	SELECT * FROM Term WHERE
		OntologyID = @ontologyId 
		AND Name LIKE @startsWith + '%' + @endsWith
		AND FREETEXT(Name,@freetextSearch)
END;
GO
/****** Object:  StoredProcedure [dbo].[fromOntologyTermNameStartsEndsWithAndContainsAndDefinitionContains]    Script Date: 6/30/2020 4:27:08 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[fromOntologyTermNameStartsEndsWithAndContainsAndDefinitionContains](
	@ontologyId				AS bigint,
	@startsWith				AS nvarchar(64), 
	@endsWith				AS nvarchar(64),
	@definitionMustContain	AS nvarchar(2048),
	@mustContain			AS nvarchar(2048)
)
AS
BEGIN 
	
	DECLARE @freetextSearchName			nvarchar(2048)
	DECLARE @freetextSearchDefinition	nvarchar(2048)
	SET @freetextSearchName			= 'FormsOf(INFLECTIONAL, "' + @mustContain + '")'
	SET @freetextSearchDefinition	= 'FormsOf(INFLECTIONAL, "' + @definitionMustContain + '")'
	
	SELECT * FROM Term WHERE
		OntologyID = @ontologyId
		AND Name LIKE @startsWith + '%' + @endsWith
		AND FREETEXT(Name,@freetextSearchName)
		AND FREETEXT(Definition,@freetextSearchDefinition)
END;
GO
/****** Object:  StoredProcedure [dbo].[fromOntologyTermNameStartsEndsWithAndDefinitionContains]    Script Date: 6/30/2020 4:27:08 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[fromOntologyTermNameStartsEndsWithAndDefinitionContains](
	@ontologyId				AS bigint,
	@startsWith				AS nvarchar(64), 
	@endsWith				AS nvarchar(64),
	@definitionMustContain	AS nvarchar(2048)
)
AS
BEGIN 
	
	DECLARE @freetextSearch nvarchar(2048)
	SET @freetextSearch = 'FormsOf(INFLECTIONAL, "' + @definitionMustContain + '")'
	SELECT * FROM Term WHERE
		OntologyID = @ontologyId
		AND Name LIKE @startsWith + '%' + @endsWith
		AND FREETEXT(Definition,@freetextSearch)
END;
GO
/****** Object:  StoredProcedure [dbo].[getAllOntologies]    Script Date: 6/30/2020 4:27:08 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[getAllOntologies]
AS
BEGIN
	SELECT * FROM Ontology
END;
GO
/****** Object:  StoredProcedure [dbo].[getTermSuggestions]    Script Date: 6/30/2020 4:27:08 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[getTermSuggestions](@query AS nvarchar(512))
AS
BEGIN
	SELECT 
		*
	FROM 
		Term 
	WHERE
		Term.Name LIKE '%' + @query+ '%'
END;
GO
/****** Object:  StoredProcedure [dbo].[getUnitTermSuggestions]    Script Date: 6/30/2020 4:27:08 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[getUnitTermSuggestions](
	@query AS nvarchar(512)
) 

AS
BEGIN

DECLARE @uo AS bigint

SELECT @uo = ID FROM Ontology
	WHERE Name = 'uo'

SELECT * FROM Term WHERE
	Term.Name LIKE '%' + @query + '%'
	AND OntologyID = @uo

END
GO
/****** Object:  StoredProcedure [dbo].[Procedure]    Script Date: 6/30/2020 4:27:08 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Procedure]
	@param1 int = 0,
	@param2 int
AS
	SELECT @param1, @param2
RETURN 0
GO
/****** Object:  StoredProcedure [dbo].[termNameStartsEndsWith]    Script Date: 6/30/2020 4:27:08 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[termNameStartsEndsWith](
	@startsWith				AS nvarchar(64), 
	@endsWith				AS nvarchar(64)
)
AS
BEGIN
	SELECT * FROM Term WHERE
		Name LIKE (@startsWith + '%' + @endsWith) 
END;
GO
/****** Object:  StoredProcedure [dbo].[termNameStartsEndsWithAndContains]    Script Date: 6/30/2020 4:27:08 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[termNameStartsEndsWithAndContains](
	@startsWith		AS nvarchar(64), 
	@endsWith		AS nvarchar(64),
	@mustContain	AS nvarchar(2048)
)
AS
BEGIN 
	
	DECLARE @freetextSearch nvarchar(2048)
	SET @freetextSearch = 'FormsOf(INFLECTIONAL, "' + @mustContain + '")'
	SELECT * FROM Term WHERE
		Name LIKE @startsWith + '%' + @endsWith
		AND FREETEXT(Name,@freetextSearch)
END;
GO
/****** Object:  StoredProcedure [dbo].[termNameStartsEndsWithAndContainsAndDefinitionContains]    Script Date: 6/30/2020 4:27:08 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[termNameStartsEndsWithAndContainsAndDefinitionContains](
	@startsWith				AS nvarchar(64), 
	@endsWith				AS nvarchar(64),
	@definitionMustContain	AS nvarchar(2048),
	@mustContain			AS nvarchar(2048)
)
AS
BEGIN 
	
	DECLARE @freetextSearchName			nvarchar(2048)
	DECLARE @freetextSearchDefinition	nvarchar(2048)
	SET @freetextSearchName			= 'FormsOf(INFLECTIONAL, "' + @mustContain + '")'
	SET @freetextSearchDefinition	= 'FormsOf(INFLECTIONAL, "' + @definitionMustContain + '")'
	
	SELECT * FROM Term WHERE
		Name LIKE @startsWith + '%' + @endsWith
		AND FREETEXT(Name,@freetextSearchName)
		AND FREETEXT(Definition,@freetextSearchDefinition)
END;
GO
/****** Object:  StoredProcedure [dbo].[termNameStartsEndsWithAndDefinitionContains]    Script Date: 6/30/2020 4:27:08 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[termNameStartsEndsWithAndDefinitionContains](
	@startsWith				AS nvarchar(64), 
	@endsWith				AS nvarchar(64),
	@definitionMustContain	AS nvarchar(2048)
)
AS
BEGIN 
	
	DECLARE @freetextSearch nvarchar(2048)
	SET @freetextSearch = 'FormsOf(INFLECTIONAL, "' + @definitionMustContain + '")'
	SELECT * FROM Term WHERE
		Name LIKE @startsWith + '%' + @endsWith
		AND FREETEXT(Definition,@freetextSearch)
END;
GO
/****** Object:  StoredProcedure [dbo].[termStartsENdsWith]    Script Date: 6/30/2020 4:27:08 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[termStartsENdsWith](
	@ontologyID				AS bigint, 
	@startsWith				AS nvarchar, 
	@endsWith				AS nvarchar
)
AS
BEGIN
	IF(@ontologyID IS NOT NULL) 
		
		SELECT * FROM Term WHERE
			OntologyID = @ontologyID 
			AND (	
				Name LIKE (@startsWith + '%' + @endsWith) 
				OR Name LIKE (@startsWith + ' %' + @endsWith) 
				OR Name LIKE (@startsWith + '% ' + @endsWith) 
				OR Name LIKE (@startsWith + ' % ' + @endsWith) 
			)
	ELSE 
		SELECT * FROM Term WHERE
			Name LIKE (@startsWith + '%' + @endsWith) 
			OR Name LIKE (@startsWith + ' %' + @endsWith) 
			OR Name LIKE (@startsWith + '% ' + @endsWith) 
			OR Name LIKE (@startsWith + ' % ' + @endsWith) 
			
END;
GO
USE [master]
GO
ALTER DATABASE [AnnotatorTest] SET  READ_WRITE 
GO
