REM Minify shaders
CALL Minify.cmd ..\Resources\Shaders ..\Resources\Shaders\Compressed

REM Minify includes
CALL Minify.cmd ..\Resources\Shaders\Inc ..\Resources\Shaders\Compressed\Inc
