#include "complex_numbers.h"
#include <cmath>
#include <iostream>
#include <stdexcept>

void check(Complex actual, Complex expected) {
    if (std::abs(actual.real - expected.real) > 1e-10 || std::abs(actual.imaginary - expected.imaginary) > 1e-10)
        throw std::runtime_error("incorrect result");
}
void print(const char* name, Complex value) {
    std::cout << name << ": " << value.real << " " << (value.imaginary < 0 ? "-" : "+") << " " << std::abs(value.imaginary) << "i\n";
}
int main() {
    Complex a{2, 3}, b{4, -1};
    check(add(a, b), {6, 2}); check(subtract(a, b), {-2, 4});
    check(multiply(a, b), {11, 10}); check(divide(a, b), {5.0 / 17, 14.0 / 17});
    print("add", add(a,b)); print("subtract", subtract(a,b));
    print("multiply", multiply(a,b)); print("divide", divide(a,b));
    bool rejected = false;
    try { divide(a, {0, 0}); } catch (const std::invalid_argument&) { rejected = true; }
    if (!rejected) return 1;
    std::cout << "PASS: four operations and division by zero\n";
}
