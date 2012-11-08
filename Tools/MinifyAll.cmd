REM Minify shaders
CALL Minify.cmd ..\Resources\Shaders ..\Resources\Shaders\Compressed

REM Minify includes
REM CALL Minify.cmd ..\Resources\Shaders\Inc ..\Resources\Shaders\Compressed\Inc
REM ========= DON'T MINIFY INCLUDES => COPY !
copy ..\Resources\Shaders\Inc ..\Resources\Shaders\Compressed\Inc



REM =======================================================
REM Aggregate individual binary blobs (*.FXBIN) into single binary shader files (*.FX)
CALL ConcatenateShaders.exe ..\Resources\Shaders\Binary\
