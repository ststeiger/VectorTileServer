
DROP TABLE region_data 
GO


CREATE TABLE region_data 
(
     continent national character varying(255) 
    ,subregion national character varying(255) 
    ,href national character varying(255) 
    ,size bigint 
    ,is_special_subregion bit 
	,spheric_area_m2 bigint 
	 
	,region_name AS 
	( 
		CASE WHEN SUBSTRING(href, CHARINDEX('/', href) + 1, LEN(href) - CHARINDEX('/', href)) LIKE '%-latest.osm.pbf' 
			THEN SUBSTRING(SUBSTRING(href, CHARINDEX('/', href) + 1, LEN(href) - CHARINDEX('/', href)), 1, LEN(SUBSTRING(href, CHARINDEX('/', href) + 1, LEN(href) - CHARINDEX('/', href))) - LEN('-latest.osm.pbf'))
			ELSE SUBSTRING(href, CHARINDEX('/', href) + 1, LEN(href) - CHARINDEX('/', href))  
		END 
	) PERSISTED 
	 
	,size_for_humans AS 
	(
		CASE
			WHEN size >= CAST(POWER(CAST(1024 AS DECIMAL(38,0)), 4) AS BIGINT) THEN CONCAT(CAST(size / CAST(POWER(CAST(1024 AS DECIMAL(38,0)), 4) AS DECIMAL(18, 2)) AS DECIMAL(18, 2)), ' TB')
			WHEN size >= CAST(POWER(CAST(1024 AS DECIMAL(38,0)), 3) AS BIGINT) THEN CONCAT(CAST(size / CAST(POWER(CAST(1024 AS DECIMAL(38,0)), 3) AS DECIMAL(18, 2)) AS DECIMAL(18, 2)), ' GB')
			WHEN size >= CAST(POWER(CAST(1024 AS DECIMAL(38,0)), 2) AS BIGINT) THEN CONCAT(CAST(size / CAST(POWER(CAST(1024 AS DECIMAL(38,0)), 2) AS DECIMAL(18, 2)) AS DECIMAL(18, 2)), ' MB')
			WHEN size >= CAST(POWER(CAST(1024 AS DECIMAL(38,0)), 1) AS BIGINT) THEN CONCAT(CAST(size / CAST(POWER(CAST(1024 AS DECIMAL(38,0)), 1) AS DECIMAL(18, 2)) AS DECIMAL(18, 2)), ' KB')
			ELSE CONCAT(CAST(size AS DECIMAL(18, 2)), ' B')
		END
	) PERSISTED

	,region_name_with_prefix AS (LEFT(href, LEN(href) - LEN('-latest.osm.pbf'))) PERSISTED 
);


/*
ALTER TABLE region_data
ADD size_for_humans AS (
    CASE
        WHEN size >= CAST(POWER(CAST(1024 AS DECIMAL(38,0)), 4) AS BIGINT) THEN CONCAT(CAST(size / CAST(POWER(CAST(1024 AS DECIMAL(38,0)), 4) AS DECIMAL(18, 2)) AS DECIMAL(18, 2)), ' TB')
        WHEN size >= CAST(POWER(CAST(1024 AS DECIMAL(38,0)), 3) AS BIGINT) THEN CONCAT(CAST(size / CAST(POWER(CAST(1024 AS DECIMAL(38,0)), 3) AS DECIMAL(18, 2)) AS DECIMAL(18, 2)), ' GB')
        WHEN size >= CAST(POWER(CAST(1024 AS DECIMAL(38,0)), 2) AS BIGINT) THEN CONCAT(CAST(size / CAST(POWER(CAST(1024 AS DECIMAL(38,0)), 2) AS DECIMAL(18, 2)) AS DECIMAL(18, 2)), ' MB')
        WHEN size >= CAST(POWER(CAST(1024 AS DECIMAL(38,0)), 1) AS BIGINT) THEN CONCAT(CAST(size / CAST(POWER(CAST(1024 AS DECIMAL(38,0)), 1) AS DECIMAL(18, 2)) AS DECIMAL(18, 2)), ' KB')
        ELSE CONCAT(CAST(size AS DECIMAL(18, 2)), ' B')
    END
) PERSISTED;
*/ 


-- ALTER TABLE region_data ADD spheric_area_m2 bigint NULL; 
