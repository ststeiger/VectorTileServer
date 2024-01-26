
-- 26.01.2024 
-- TRUNCATE TABLE region_data; 


SELECT * FROM region_data; 


SELECT 
	 continent 
	,subregion 
	,size 
FROM region_data 

ORDER BY Size DESC 
;


SELECT 
	 continent 
	,subregion 
	,region_name 
	,size 
FROM region_data 
WHERE (1=1) 

-- AND is_special_subregion = 0 
-- AND continent = 'europe' 
-- AND continent = 'australia' 

ORDER BY size 




SELECT continent FROM region_data GROUP BY continent; 
-- africa, asia, australia-oceania, central-america, 
-- europe, north-america, south-america
-- earth (=continents) 





SELECT 
     subregion 
	,size 
	,size_for_humans 
    ,CAST(ROUND(size * 100.0 / SUM(size) OVER (), 2) AS decimal(5,2)) AS percentage_of_total 
	,SUM(size) OVER () AS total_size  

	,
	CASE
		WHEN SUM(size) OVER () >= CAST(POWER(CAST(1024 AS DECIMAL(38,0)), 4) AS BIGINT) THEN CONCAT(CAST(SUM(size) OVER () / CAST(POWER(CAST(1024 AS DECIMAL(38,0)), 4) AS DECIMAL(18, 2)) AS DECIMAL(18, 2)), ' TB')
		WHEN SUM(size) OVER () >= CAST(POWER(CAST(1024 AS DECIMAL(38,0)), 3) AS BIGINT) THEN CONCAT(CAST(SUM(size) OVER () / CAST(POWER(CAST(1024 AS DECIMAL(38,0)), 3) AS DECIMAL(18, 2)) AS DECIMAL(18, 2)), ' GB')
		WHEN SUM(size) OVER () >= CAST(POWER(CAST(1024 AS DECIMAL(38,0)), 2) AS BIGINT) THEN CONCAT(CAST(SUM(size) OVER () / CAST(POWER(CAST(1024 AS DECIMAL(38,0)), 2) AS DECIMAL(18, 2)) AS DECIMAL(18, 2)), ' MB')
		WHEN SUM(size) OVER () >= CAST(POWER(CAST(1024 AS DECIMAL(38,0)), 1) AS BIGINT) THEN CONCAT(CAST(SUM(size) OVER () / CAST(POWER(CAST(1024 AS DECIMAL(38,0)), 1) AS DECIMAL(18, 2)) AS DECIMAL(18, 2)), ' KB')
		ELSE CONCAT(CAST(SUM(size) OVER () AS DECIMAL(18, 2)), ' B')
	END AS total_size_human 
FROM region_data 
WHERE continent = 'earth' 
ORDER BY size DESC 
;




;WITH cte_ranked_regions AS 
( 
    SELECT 
         continent 
		,subregion 
		,region_name
		,size 
		,ROW_NUMBER() OVER (PARTITION BY Continent ORDER BY Size) AS row_num  
    FROM region_data 
) 
SELECT 
     row_num 
	,continent 
	,subregion 
	-- ,region_name 
	,size 
FROM cte_ranked_regions 
WHERE row_num < 4 

ORDER BY row_num, size 
; 
