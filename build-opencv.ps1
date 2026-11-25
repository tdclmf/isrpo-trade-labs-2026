param([Parameter(Mandatory=$true)][string]$WorkDirectory)
$ErrorActionPreference='Stop'
if($WorkDirectory -match '[^\x00-\x7F]') {throw 'Для сборки OpenCV укажите каталог с латинским путём'}
New-Item -ItemType Directory -Path $WorkDirectory -Force | Out-Null
$archive=Join-Path $WorkDirectory 'opencv-4.11.0.zip'
$source=Join-Path $WorkDirectory 'opencv-4.11.0'
$build=Join-Path $WorkDirectory 'opencv_build'
$install=Join-Path $WorkDirectory 'opencv_install'
if(-not(Test-Path -LiteralPath $source)) {
    Invoke-WebRequest 'https://github.com/opencv/opencv/archive/refs/tags/4.11.0.zip' -OutFile $archive
    Expand-Archive -LiteralPath $archive -DestinationPath $WorkDirectory
}
cmake -S $source -B $build -G 'Visual Studio 17 2022' -A x64 "-DCMAKE_INSTALL_PREFIX=$install" '-DBUILD_LIST=core,imgproc,imgcodecs,objdetect,highgui' -DBUILD_SHARED_LIBS=OFF -DBUILD_TESTS=OFF -DBUILD_PERF_TESTS=OFF -DBUILD_EXAMPLES=OFF -DBUILD_opencv_apps=OFF -DBUILD_opencv_python3=OFF -DBUILD_JAVA=OFF -DWITH_IPP=OFF -DWITH_OPENCL=OFF -DWITH_FFMPEG=OFF -DWITH_MSMF=OFF -DWITH_PROTOBUF=OFF -DBUILD_PROTOBUF=OFF -DWITH_ADE=OFF -DWITH_OPENJPEG=OFF -DWITH_WEBP=OFF -DWITH_TIFF=OFF -DWITH_OPENEXR=OFF '-DCPU_DISPATCH=' '-DCMAKE_CXX_FLAGS=/utf-8 /EHsc' '-DCMAKE_C_FLAGS=/utf-8'
if($LASTEXITCODE -ne 0) {throw 'Ошибка конфигурации OpenCV'}
cmake --build $build --config Release --target INSTALL --parallel 4
if($LASTEXITCODE -ne 0) {throw 'Ошибка сборки OpenCV'}
Write-Output "OpenCV_DIR=$install/x64/vc17/staticlib"
