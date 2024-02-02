
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




-- Falsch, eigent sich aber für unterregionale Daten.
--SELECT 
--	 continent 
--	-- ,SUM(size) AS size 
	 
--	,CASE 
--		WHEN SUM(size)>=CONVERT(bigint,POWER(CONVERT(decimal(38,0),(1024)),(4))) 
--			THEN CONCAT(CONVERT(decimal(18,2),SUM(size)/CONVERT(decimal(18,2),POWER(CONVERT(decimal(38,0),(1024)),(4)))),' TB') 
--		WHEN SUM(size)>=CONVERT(bigint,POWER(CONVERT(decimal(38,0),(1024)),(3))) 
--			THEN CONCAT(CONVERT(decimal(18,2),SUM(size)/CONVERT(decimal(18,2),POWER(CONVERT(decimal(38,0),(1024)),(3)))),' GB') 
--		WHEN SUM(size)>=CONVERT(bigint,POWER(CONVERT(decimal(38,0),(1024)),(2))) 
--			THEN CONCAT(CONVERT(decimal(18,2),SUM(size)/CONVERT(decimal(18,2),POWER(CONVERT(decimal(38,0),(1024)),(2)))),' MB') 
--		WHEN SUM(size)>=CONVERT(bigint,POWER(CONVERT(decimal(38,0),(1024)),(1))) 
--			THEN CONCAT(CONVERT(decimal(18,2),SUM(size)/CONVERT(decimal(18,2),POWER(CONVERT(decimal(38,0),(1024)),(1)))),' KB') 
--		ELSE CONCAT(CONVERT(decimal(18,2),SUM(size)),' B') 
--	 END AS size_for_humans 

--	,CAST(ROUND(SUM(size) * 100.0 / SUM(SUM(size)) OVER (), 2) AS decimal(5,2)) AS percentage_of_total 
--FROM region_data 

--WHERE (1=1) 
--AND continent <> 'earth' 

--GROUP BY continent 

--ORDER BY SUM(size) DESC 





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




SELECT 
	 continent 
	,subregion 
	-- ,size 
	-- , 
	,region_name 
	,size_for_humans 
	,size 
	,spheric_area_m2 
	 
	 -- I call this the "OpenStreetMap Development coefficient" 
	,CAST(size AS float) / CAST(spheric_area_m2 AS float) AS area_size_ratio 
FROM region_data 
WHERE (1=1) 
-- AND region_name = 'russia' -- yes, it's there even if continent "russia" is excluded 

AND continent <> 'earth'  
AND continent NOT IN ('us', 'brazil', 'canada', 'russia') 
AND COALESCE(is_special_subregion, 0) = 0 
-- AND continent NOT IN ('africa', 'asia', 'australia-oceania', 'europe' , 'north-america', 'central-america', 'south-america') 
-- AND continent NOT IN ( 'africa', 'australia-oceania', 'asia', 'europe', 'north-america', 'central-america', 'south-america', 'brazil', 'canada', 'us', 'russia' )
-- AND continent = 'africa'  
-- AND continent = 'asia' 
-- AND continent = 'europe'  
-- AND continent = 'us' 
-- AND continent = 'canada' 
-- AND continent = 'north-america' 
-- AND continent = 'south-america' 
-- AND continent = 'central-america' 
-- AND continent = 'australia-oceania' 

-- not an island 
AND subregion NOT IN 
(
	
	'Bahamas'
	,'Fiji'
	,'Comores'
	,'Prince Edward Island'
	,'Nova Scotia'
	 
	,'Azores'
	,'Faroe Islands'
	,'Canary Islands'
	,'Isle of Man'
	,'Guernsey and Jersey'
	 
	 -- ,'Cyprus' 
	 -- ,'Malta' 
	 -- ,'Iceland' 
	-- ,'Ireland and Northern Ireland' 
	-- ,'Britain and Ireland' 
	-- ,'Australia' -- yes, it's an island, and too much desert on it ;) 
	-- ,'New Zealand' 
	-- ,'Rhode Island' 
	 
	-- ,'Puerto Rico'
	,'US Pacific' -- Alaska and Hawai 
	 
	-- ,'Cuba'
	-- ,'Haiti and Dominican Republic'
	-- ,'Sri Lanka'
	-- ,'Philippines'
	-- ,'Indonesia (with East Timor)'

	-- 'Madagascar'

	--,'Crimean Federal District'
	-- ,'Taiwan'

	,'Kiribati'
	,'Samoa'
	,'Sao Tome and Principe'
	,'Papua New Guinea'
	,'New Caledonia'
	,'Maldives'
	,'Vanuatu'
	,'Mauritius'
	,'Tonga'
	,'Hawaii'
	,'Solomon Islands'
	,'Greenland'
	,'American Oceania'
	,'Seychelles'
	,'Polynésie française (French Polynesia)'
	,'Tuvalu'
	,'Wallis et Futuna'
	,'Palau'
	,'Niue'
	,'Île de Clipperton'
	,'Nauru'
	,'Marshall Islands'
	,'Cook Islands'
	,'Micronesia'
	,'Tokelau'
	,'Saint Helena, Ascension, and Tristan da Cunha'
	,'Pitcairn Islands'
)


ORDER BY 
	 CASE WHEN spheric_area_m2 < 0 THEN 0 ELSE 1 END  
	 -- ,continent 
	 -- ,subregion 
	 -- ,size 
	,area_size_ratio DESC 
	 