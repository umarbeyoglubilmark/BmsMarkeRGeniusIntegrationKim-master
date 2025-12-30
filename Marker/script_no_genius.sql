USE [ERULKU]
GO
/****** Object:  UserDefinedFunction [dbo].[BMS_FNC_PennaGeniusIntegration_CharacterFix] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE OR ALTER FUNCTION [dbo].[BMS_FNC_PennaGeniusIntegration_CharacterFix]
(
    @InputString NVARCHAR(MAX)
)
RETURNS NVARCHAR(MAX)
AS
BEGIN
    DECLARE @OutputString NVARCHAR(MAX) = @InputString

    -- Replace Turkish characters with ASCII equivalents using NCHAR codes
    SET @OutputString = REPLACE(@OutputString, NCHAR(305), N'i')  -- dotless i
    SET @OutputString = REPLACE(@OutputString, NCHAR(304), N'I')  -- dotted I
    SET @OutputString = REPLACE(@OutputString, NCHAR(287), N'g')  -- g with breve
    SET @OutputString = REPLACE(@OutputString, NCHAR(286), N'G')  -- G with breve
    SET @OutputString = REPLACE(@OutputString, NCHAR(252), N'u')  -- u with umlaut
    SET @OutputString = REPLACE(@OutputString, NCHAR(220), N'U')  -- U with umlaut
    SET @OutputString = REPLACE(@OutputString, NCHAR(351), N's')  -- s with cedilla
    SET @OutputString = REPLACE(@OutputString, NCHAR(350), N'S')  -- S with cedilla
    SET @OutputString = REPLACE(@OutputString, NCHAR(246), N'o')  -- o with umlaut
    SET @OutputString = REPLACE(@OutputString, NCHAR(214), N'O')  -- O with umlaut
    SET @OutputString = REPLACE(@OutputString, NCHAR(231), N'c')  -- c with cedilla
    SET @OutputString = REPLACE(@OutputString, NCHAR(199), N'C')  -- C with cedilla

    -- Remove other problematic characters
    SET @OutputString = REPLACE(@OutputString, CHAR(0), N'')
    SET @OutputString = REPLACE(@OutputString, CHAR(9), N' ')
    SET @OutputString = REPLACE(@OutputString, CHAR(10), N' ')
    SET @OutputString = REPLACE(@OutputString, CHAR(13), N' ')

    RETURN @OutputString
END
GO
USE [ERULKU]
GO
/****** Object:  UserDefinedFunction [dbo].[Bmsf_125_MarkeRGeniusIntegration_Malzemeler]    Script Date: 25.12.2025 11:03:28 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO













 


CREATE OR ALTER FUNCTION [dbo].[Bmsf_125_MarkeRGeniusIntegration_Malzemeler] (
    @WHOUSENR int
)
RETURNS TABLE
AS
RETURN 
SELECT 
	* 
FROM (
SELECT
TARIH=cast(CASE	WHEN (SELECT TOP 1 ISNULL(PL.CHANGEDATE,PL.RECDATE) FROM LK_125_PRCLIST PL WITH(NOLOCK) WHERE PL.LOGICALREF=LK_PRCLISTREF)>
			ITEMDATE
				THEN (SELECT TOP 1 ISNULL(PL.CHANGEDATE,PL.RECDATE) FROM LK_125_PRCLIST PL WITH(NOLOCK) WHERE PL.LOGICALREF=LK_PRCLISTREF) ELSE
			ITEMDATE
		END as date), 
IBMGENIUSAMBAR
,CODE
,BARCODE
,ALCOHOL = (CASE WHEN CYPHCODE LIKE '%İÇKİ%' THEN 1 ELSE 0 END)
,EXPLANATION=dbo.BMS_FNC_PennaGeniusIntegration_CharacterFix(EXPLANATION)
,UNIT1
,UNIT1IBM=(CASE UNIT1 WHEN 'ADET' THEN '1' WHEN 'M' THEN '100' WHEN 'KG' THEN '1000' END)
,SELLING_PRICE1=	ROUND((SELECT TOP 1 PL.BUYPRICE FROM LK_125_PRCLIST PL WITH(NOLOCK) WHERE PL.LOGICALREF=LK_PRCLISTREF),2)
,VAT_RATE
,VAT_CODE
,VAT_CODE_N
,GROUPID =
    CASE 
	       
	        WHEN ISNULL((SELECT TOP 1 M.CODE FROM LG_125_MARK M WHERE M.LOGICALREF=MARKREF),'') = '262' THEN '1'     
        WHEN ISNULL((SELECT TOP 1 M.CODE FROM LG_125_MARK M WHERE M.LOGICALREF=MARKREF),'') = '227' THEN '2'                     -- 113 ürün
                -- 585 ürün
        WHEN ISNULL((SELECT TOP 1 M.CODE FROM LG_125_MARK M WHERE M.LOGICALREF=MARKREF),'') = '1161' THEN '3'                -- 431 ürün
        WHEN ISNULL((SELECT TOP 1 M.CODE FROM LG_125_MARK M WHERE M.LOGICALREF=MARKREF),'') = '1167' THEN '4'                   -- 301 ürün
        WHEN SPECODE2 = '11104' THEN '5'              -- 158 ürün
        WHEN SPECODE2 = '14202' THEN '6'          -- 775 ürün
        WHEN ISNULL((SELECT TOP 1 M.CODE FROM LG_125_MARK M WHERE M.LOGICALREF=MARKREF),'') = '1176' AND SPECODE3 = '776' THEN '7'  -- 157 ürün
        WHEN ISNULL((SELECT TOP 1 M.CODE FROM LG_125_MARK M WHERE M.LOGICALREF=MARKREF),'') = '1176' AND SPECODE3 = '137' THEN '8'  
		
		-- 309 ürün
        ELSE '0' 
    END
,GROUPINFO =
    CASE 
        WHEN ISNULL((SELECT TOP 1 M.CODE FROM LG_125_MARK M WHERE M.LOGICALREF=MARKREF),'') = '227' THEN 'Arian kampanya'                     -- 113 ürün
        WHEN ISNULL((SELECT TOP 1 M.CODE FROM LG_125_MARK M WHERE M.LOGICALREF=MARKREF),'') = '262' THEN 'Arzum kampanya'                     -- 585 ürün
        WHEN ISNULL((SELECT TOP 1 M.CODE FROM LG_125_MARK M WHERE M.LOGICALREF=MARKREF),'') = '1161' THEN 'Hascevher kampanya'                -- 431 ürün
        WHEN ISNULL((SELECT TOP 1 M.CODE FROM LG_125_MARK M WHERE M.LOGICALREF=MARKREF),'') = '1167' THEN 'Mehtap kampanya'                   -- 301 ürün
        WHEN SPECODE2 = '11104' THEN 'Bisiklet kampanyası'              -- 158 ürün
       WHEN SPECODE2 = '14202' THEN 'Twigy Terlik Grubu Yeni Yıl Kampanyası'     -- 775 ürün
        WHEN ISNULL((SELECT TOP 1 M.CODE FROM LG_125_MARK M WHERE M.LOGICALREF=MARKREF),'') = '1176' AND SPECODE3 = '776' THEN 'Wk 776 kampanyası'  -- 157 ürün
        WHEN ISNULL((SELECT TOP 1 M.CODE FROM LG_125_MARK M WHERE M.LOGICALREF=MARKREF),'') = '1176' AND SPECODE3 = '137' THEN 'Wk 137 kampanya'    -- 309 ürün
        ELSE '' 
    END
,MARKCODE=ISNULL((SELECT TOP 1 M.CODE FROM LG_125_MARK M WHERE M.LOGICALREF=MARKREF),'')

,SPECODE
,SPECODE2
,SPECODE3
,SPECODE4
,SPECODE5
,ACTIVE
 FROM (
SELECT 
	IBMGENIUSAMBAR=(SELECT TOP 1 KDD.OFFICECODE FROM  LK_125_DIVDEFAULTS KDD WITH(NOLOCK) WHERE KDD.WHOUSENR=@WHOUSENR)
	,ITEMREF=		I.LOGICALREF
	,ITEMDATE=		CASE WHEN ISNULL(I.CAPIBLOCK_MODIFIEDDATE,CONVERT(DATETIME, 0))>ISNULL(I.CAPIBLOCK_CREADEDDATE,'20230101') THEN I.CAPIBLOCK_MODIFIEDDATE ELSE I.CAPIBLOCK_CREADEDDATE END
	,I.CODE
	,I.MARKREF
	,B.BARCODE
	,EXPLANATION=	LEFT(I.NAME,20)
	,UNIT1=			(SELECT TOP 1 UL.CODE FROM LG_125_UNITSETL UL  WITH(NOLOCK) WHERE UL.MAINUNIT=1 AND UL.UNITSETREF=I.UNITSETREF)
	,LK_PRCLISTREF=	ISNULL((SELECT TOP 1 KF.LOGICALREF FROM 
						LK_125_PRCLIST KF WITH(NOLOCK)  WHERE KF.OFFICECODE=(SELECT KDD.OFFICECODE FROM 
							LK_125_DIVDEFAULTS KDD  WITH(NOLOCK) WHERE KDD.WHOUSENR=@WHOUSENR) AND KF.STREF=I.LOGICALREF AND KF.VARIANTREF=0 
								ORDER BY KF.LOGICALREF DESC),0)
	,VAT_RATE=I.SELLPRVAT
	,ACTIVE = I.ACTIVE
	,VAT_CODE=(SELECT TOP 1 M.KDVDEPNR FROM  LG_125_MARKET M WITH(NOLOCK)  WHERE M.ITEMREF=I.LOGICALREF)
	,VAT_CODE_N=(
	SELECT TOP 1 
    CASE M.KDVDEPNR
        WHEN 5 THEN 8
        WHEN 7 THEN 1
        WHEN 1 THEN 2
        WHEN 6 THEN 3
        WHEN 3 THEN 4
        WHEN 4 THEN 5
        ELSE M.KDVDEPNR
    END AS NCR
FROM LG_125_MARKET M WITH(NOLOCK)
WHERE M.ITEMREF = I.LOGICALREF
	)
	,SPECODE
	,SPECODE2
	,SPECODE3
	,SPECODE4
	,SPECODE5
	,CYPHCODE
FROM 
	LG_125_UNITBARCODE B  WITH(NOLOCK) LEFT JOIN LG_125_ITEMS I WITH(NOLOCK)  ON I.LOGICALREF = B.ITEMREF 
) AS TT
) AS TF
WHERE 
	TF.SELLING_PRICE1>0 AND YEAR(TF.TARIH)>=2023
GO
/****** Object:  View [dbo].[BMS_125_MarkeRGenius_HatalıÜrünKarakterleri]    Script Date: 25.12.2025 11:03:28 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE OR ALTER VIEW [dbo].[BMS_125_MarkeRGenius_HatalıÜrünKarakterleri] AS
SELECT TARIH,EXPLANATION,CODE,
 patindex('%[^! -~]%' COLLATE Latin1_General_BIN,EXPLANATION) as [Position],
 substring(EXPLANATION,patindex('%[^ !-~]%' COLLATE Latin1_General_BIN,EXPLANATION),1) AS [InvalidCharacter],
 ascii(substring(EXPLANATION,patindex('%[^ !-~]%' COLLATE Latin1_General_BIN,EXPLANATION),1)) as [ASCIICode]
 from Bmsf_125_MarkeRGeniusIntegration_Malzemeler(0)
 where patindex('%[^! -~]%' COLLATE Latin1_General_BIN,EXPLANATION)>0 AND TARIH > '2025-04-01'



GO
/****** Object:  View [dbo].[BMS_125_MarkeRGenius_Branch]    Script Date: 25.12.2025 11:03:28 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO




CREATE OR ALTER VIEW [dbo].[BMS_125_MarkeRGenius_Branch] as 
select NR,NAME from L_CAPIDIV WHERE FIRMNR=125
GO
/****** Object:  View [dbo].[BMS_125_MarkeRGenius_DebtClose_Csroll]    Script Date: 25.12.2025 11:03:28 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER VIEW [dbo].[BMS_125_MarkeRGenius_DebtClose_Csroll]  as 
SELECT 
	PAYTRANS_CSROLL=	PT.LOGICALREF
	,BRANCH=			I.BRANCH
	,DOCODE=			I.DOCODE
	,DATE_CSROLL=		I.DATE_
	,CLIENTREF=			PT.CARDREF
	,I.SPECODE
	,PAYTRANS_TOTAL=	PT.TOTAL 
FROM 
	LG_125_01_PAYTRANS PT LEFT JOIN LG_125_01_CSROLL I ON I.LOGICALREF=PT.FICHEREF
WHERE
	 PT.MODULENR=6 AND PT.TRCODE IN (1) AND PT.PAID=0 AND I.CYPHCODE='BMS' AND I.CAPIBLOCK_CREATEDBY=1 AND PT.CARDREF NOT IN (SELECT C.LOGICALREF FROM LG_125_CLCARD C WHERE C.CODE   IN ('Z.001','Z.002') )
GO
/****** Object:  View [dbo].[BMS_125_MarkeRGenius_DebtClose_Invoice]    Script Date: 25.12.2025 11:03:28 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER VIEW [dbo].[BMS_125_MarkeRGenius_DebtClose_Invoice]  as 
SELECT 
	PAYTRANS_INVOICE=	PT.LOGICALREF
	,BRANCH=			I.BRANCH
	,DOCODE=			I.DOCODE
	,DATE_INVOICE=		I.DATE_
	,CLIENTREF=			PT.CARDREF
	,I.SPECODE
	,PAYTRANS_TOTAL=	PT.TOTAL 
FROM 
	LG_125_01_PAYTRANS PT LEFT JOIN LG_125_01_INVOICE I ON I.LOGICALREF=PT.FICHEREF
WHERE
	 PT.MODULENR=4 AND PT.TRCODE IN (7) AND PT.PAID=0 AND I.CYPHCODE='BMS' AND I.POSTRANSFERINFO=1 AND I.CAPIBLOCK_CREATEDBY=1 AND PT.CARDREF NOT IN (SELECT C.LOGICALREF FROM LG_125_CLCARD C WHERE C.CODE   IN ('Z.001','Z.002') )
GO
/****** Object:  View [dbo].[BMS_125_MarkeRGenius_DebtClose_Kslines]    Script Date: 25.12.2025 11:03:28 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER VIEW [dbo].[BMS_125_MarkeRGenius_DebtClose_Kslines] as 
SELECT 
	PAYTRANS_KSLINES=	PT.LOGICALREF
	,BRANCH=			I.BRANCH
	,DOCODE=			I.DOCODE
	,DATE_KSLINES=		I.DATE_
	,CLIENTREF=			PT.CARDREF
	,I.SPECODE
	,PAYTRANS_TOTAL=	PT.TOTAL 
FROM LG_125_01_PAYTRANS PT LEFT JOIN LG_125_01_KSLINES I ON I.LOGICALREF=PT.FICHEREF
WHERE PT.MODULENR=10 AND PT.TRCODE IN (1) AND PT.PAID=0 AND I.CYPHCODE='BMS' AND I.CAPIBLOCK_CREATEDBY=1 AND PT.CARDREF NOT IN (SELECT C.LOGICALREF FROM LG_125_CLCARD C WHERE C.CODE   IN ('Z.001','Z.002'));
GO
/****** Object:  View [dbo].[BMS_125_MarkeRGenius_HatalıÜrünBarkodları]    Script Date: 25.12.2025 11:03:28 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER VIEW [dbo].[BMS_125_MarkeRGenius_HatalıÜrünBarkodları] as
SELECT BARCODE as barkod ,
(
CASE 
WHEN LEN(BARCODE)>18 THEN '18 Karakterden Uzun'
 end
)
as [Hata Detay]
FROM LG_125_UNITBARCODE where LEN(BARCODE) > 18
GO
/****** Object:  View [dbo].[BMS_125_MarkeRGenius_InvoiceClient]    Script Date: 25.12.2025 11:03:28 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO







CREATE OR ALTER VIEW [dbo].[BMS_125_MarkeRGenius_InvoiceClient] as 
select CODE AS NR, DEFINITION_ AS NAME from LG_125_CLCARD where CODE='Z.001'
GO
/****** Object:  View [dbo].[BMS_125_MarkeRGenius_ReturnClient]    Script Date: 25.12.2025 11:03:28 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO






CREATE OR ALTER VIEW [dbo].[BMS_125_MarkeRGenius_ReturnClient] as 
select CODE AS NR, DEFINITION_ AS NAME from LG_125_CLCARD where CODE='Z.001'
GO
/****** Object:  View [dbo].[Bms_125_MarkeRGeniusIntegration_Cariler]    Script Date: 25.12.2025 11:03:28 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER VIEW [dbo].[Bms_125_MarkeRGeniusIntegration_Cariler] as
SELECT 
TARIH=CAST(CASE WHEN ISNULL(I.CAPIBLOCK_MODIFIEDDATE,I.CAPIBLOCK_CREADEDDATE) > I.CAPIBLOCK_CREADEDDATE THEN I.CAPIBLOCK_MODIFIEDDATE ELSE I.CAPIBLOCK_CREADEDDATE END  AS DATE)
	,CARDREF=		I.LOGICALREF
	,I.CODE
	--,B.BARCODE
	,EXPLANATION=	DBO.BMS_FNC_PennaGeniusIntegration_CharacterFix(I.DEFINITION_ )
	,TELNR=	LEFT(TELNRS1+' '+TELNRS2,20)
	,SPECODE5 AS INDIRIM
FROM 
	LG_125_CLCARD I WITH(NOLOCK)  
WHERE
	ACTIVE=0 AND LOGICALREF>1
GO
/****** Object:  Table [dbo].[Bms_125_MarkeRGeniusIntegration_Default]    Script Date: 25.12.2025 11:03:28 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF OBJECT_ID('dbo.Bms_125_MarkeRGeniusIntegration_Default', 'U') IS NOT NULL DROP TABLE [dbo].[Bms_125_MarkeRGeniusIntegration_Default]
GO
CREATE TABLE [dbo].[Bms_125_MarkeRGeniusIntegration_Default](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Description] [nvarchar](50) NULL,
	[Value] [nvarchar](50) NULL,
 CONSTRAINT [PK_Bms_125_MarkeRGeniusIntegration_Default] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Bms_125_MarkeRGeniusIntegration_IbmKasa]    Script Date: 25.12.2025 11:03:28 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF OBJECT_ID('dbo.Bms_125_MarkeRGeniusIntegration_IbmKasa', 'U') IS NOT NULL DROP TABLE [dbo].[Bms_125_MarkeRGeniusIntegration_IbmKasa]
GO
CREATE TABLE [dbo].[Bms_125_MarkeRGeniusIntegration_IbmKasa](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[LogoValue] [nvarchar](199) NULL,
	[G3Value] [nvarchar](199) NULL,
	[Path] [nvarchar](max) NULL,
	[SqlServer] [nvarchar](199) NULL,
	[SqlUsername] [nvarchar](199) NULL,
	[SqlPassword] [nvarchar](199) NULL,
	[SqlDatabase] [nvarchar](199) NULL,
 CONSTRAINT [PK_Bms_125_PennaGeniusIntegration_IbmKasa] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Bms_125_MarkeRGeniusIntegration_IbmKasa_Yedek]    Script Date: 25.12.2025 11:03:28 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF OBJECT_ID('dbo.Bms_125_MarkeRGeniusIntegration_IbmKasa_Yedek', 'U') IS NOT NULL DROP TABLE [dbo].[Bms_125_MarkeRGeniusIntegration_IbmKasa_Yedek]
GO
CREATE TABLE [dbo].[Bms_125_MarkeRGeniusIntegration_IbmKasa_Yedek](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[LogoValue] [nvarchar](199) NULL,
	[G3Value] [nvarchar](199) NULL,
	[Path] [nvarchar](max) NULL,
	[SqlServer] [nvarchar](199) NULL,
	[SqlUsername] [nvarchar](199) NULL,
	[SqlPassword] [nvarchar](199) NULL,
	[SqlDatabase] [nvarchar](199) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Bms_125_MarkeRGeniusIntegration_Mapping]    Script Date: 25.12.2025 11:03:28 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF OBJECT_ID('dbo.Bms_125_MarkeRGeniusIntegration_Mapping', 'U') IS NOT NULL DROP TABLE [dbo].[Bms_125_MarkeRGeniusIntegration_Mapping]
GO
CREATE TABLE [dbo].[Bms_125_MarkeRGeniusIntegration_Mapping](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[LogoBranch] [nvarchar](50) NULL,
	[PosBranch] [nvarchar](50) NULL,
	[Ip] [nvarchar](50) NULL,
 CONSTRAINT [PK_Bms_125_MarkeRGeniusIntegration_Mapping] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Bms_125_MarkeRGeniusIntegration_PaymentMapping]    Script Date: 25.12.2025 11:03:28 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF OBJECT_ID('dbo.Bms_125_MarkeRGeniusIntegration_PaymentMapping', 'U') IS NOT NULL DROP TABLE [dbo].[Bms_125_MarkeRGeniusIntegration_PaymentMapping]
GO
CREATE TABLE [dbo].[Bms_125_MarkeRGeniusIntegration_PaymentMapping](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Branch] [int] NULL,
	[Saleman] [nvarchar](50) NULL,
	[IntegrationCode] [nvarchar](50) NULL,
	[LogoFicheType] [nvarchar](50) NULL,
	[Currency] [nvarchar](50) NULL,
	[BankOrKsCode] [nvarchar](50) NULL,
 CONSTRAINT [PK_Bms_125_MarkeRGeniusIntegration_PaymentMapping] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

