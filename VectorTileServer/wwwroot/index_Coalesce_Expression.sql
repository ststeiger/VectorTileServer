
;WITH CTE AS 
(
	SELECT 'Liechtenstein' AS "name:de", 'Liechtenstein' AS "name:latin", '' AS "name:nonlatin"

	UNION ALL 

	SELECT '' AS "name:de", 'Stein am Rhein' AS "name:latin", '' AS "name:nonlatin"

	UNION ALL 

	SELECT 'Sonders' AS "name:de", 'Sondrio' AS "name:latin", '' AS "name:nonlatin"
)
SELECT 
	CONCAT
	(
		 NULLIF(COALESCE("name:de", "name:latin", "name:nonlatin"), '')
		, 
		CASE WHEN NULLIF(COALESCE("name:de", "name:latin", "name:nonlatin"), '') IS NOT NULL THEN '(' ELSE '' END 
		+ NULLIF
		(
			 COALESCE("name:latin", "name:nonlatin") 
			,NULLIF(COALESCE("name:de", "name:latin", "name:nonlatin"), '')
		)
		+ CASE WHEN NULLIF(COALESCE("name:de", "name:latin", "name:nonlatin"), '') IS NOT NULL THEN ')' ELSE '' END 
	) 
FROM CTE 



-- ChatGPT-Version: 


;WITH CTE2 AS 
(
	SELECT 'Liechtenstein' AS "name:de", 'Liechtenstein' AS "name:latin", '' AS "name:nonlatin"

	UNION ALL 

	SELECT '' AS "name:de", 'Stein am Rhein' AS "name:latin", '' AS "name:nonlatin"

	UNION ALL 

	SELECT 'Sonders' AS "name:de", 'Sondrio' AS "name:latin", '' AS "name:nonlatin"
)
,CTE AS 
( 
	SELECT 
		 "name:de" AS preferred_name 
		,"name:latin" AS name_latin 
		,"name:nonlatin" AS name_nonlatin 
	FROM CTE2 
) 
SELECT 
	CASE
	  WHEN COALESCE(preferred_name, name_latin, name_nonlatin) IS NOT NULL THEN
		CASE
		  WHEN preferred_name IS NOT NULL AND preferred_name <> ''
		  THEN
			CASE
			  WHEN name_latin IS NOT NULL AND name_latin <> ''
				   AND preferred_name <> name_latin
				   AND name_latin NOT LIKE '%/%'
				   AND name_latin NOT LIKE '%-%'
			  THEN preferred_name + ' (' + name_latin + ')'
			  ELSE preferred_name
			END
		  ELSE COALESCE(name_latin, name_nonlatin, '')
		END
	  ELSE ''
	END AS chatgpt_version 
FROM CTE 
