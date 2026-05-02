#include <iostream>
#include <string>

// A sample C++ file for testing text extraction
class SampleClass {
public:
    std::string name;

    int calculate(int a, int b) {
        return a + b;
    }
};

int main() {
    SampleClass obj;
    std::cout << obj.calculate(3, 4) << std::endl;
    return 0;
}
