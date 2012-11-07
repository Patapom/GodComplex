@REM Minifies all files from directory %1 into identical files in directory %2
@REM Source from http://www.jamesewelch.com/2008/05/01/how-to-write-a-dos-batch-file-to-loop-through-files/

for /f %%a IN ('dir /b %1\*.fx') do shader_minifier.exe %1\%%a --hlsl --shader-only --preserve-externals --field-names xyzw --no-renaming-list VS,HS,DS,GS,PS,Distort -o %2\%%a
