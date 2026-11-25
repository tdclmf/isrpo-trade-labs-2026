#include "complex_numbers.h"
#include <complex>
#include <stdexcept>

Complex add(Complex a, Complex b) { return {a.real + b.real, a.imaginary + b.imaginary}; }
Complex subtract(Complex a, Complex b) { return {a.real - b.real, a.imaginary - b.imaginary}; }
Complex multiply(Complex a, Complex b) {
    return {a.real * b.real - a.imaginary * b.imaginary, a.real * b.imaginary + a.imaginary * b.real};
}
Complex divide(Complex a, Complex b) {
    if (b.real == 0 && b.imaginary == 0) throw std::invalid_argument("division by zero");
    const auto result = std::complex<double>(a.real, a.imaginary) / std::complex<double>(b.real, b.imaginary);
    return {result.real(), result.imag()};
}
