param([string]$OpenCV_DIR)
$ErrorActionPreference='Stop'
cmake -S "$PSScriptRoot\ComplexNumbers" -B "$PSScriptRoot\ComplexNumbers\_build" -G 'Visual Studio 17 2022' -A x64
if($LASTEXITCODE -ne 0) {throw 'Ошибка конфигурации ComplexNumbers'}
cmake --build "$PSScriptRoot\ComplexNumbers\_build" --config Release
if($LASTEXITCODE -ne 0) {throw 'Ошибка сборки ComplexNumbers'}
ctest --test-dir "$PSScriptRoot\ComplexNumbers\_build" -C Release --output-on-failure
if($LASTEXITCODE -ne 0) {throw 'Проверка комплексных чисел не пройдена'}
if($OpenCV_DIR) {
    cmake -S "$PSScriptRoot\FaceDetection" -B "$PSScriptRoot\FaceDetection\_build" -G 'Visual Studio 17 2022' -A x64 "-DOpenCV_DIR=$OpenCV_DIR"
    if($LASTEXITCODE -ne 0) {throw 'Ошибка конфигурации FaceDetection'}
    cmake --build "$PSScriptRoot\FaceDetection\_build" --config Release
    if($LASTEXITCODE -ne 0) {throw 'Ошибка сборки FaceDetection'}
}
