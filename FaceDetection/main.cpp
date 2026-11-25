#include <opencv2/opencv.hpp>
#include <iostream>
#include <vector>

int main(int argc, char** argv) {
    if (argc < 4) { std::cerr << "Usage: face_detection image cascade output [--show]\n"; return 2; }
    cv::Mat image = cv::imread(argv[1]);
    cv::CascadeClassifier classifier;
    if (image.empty() || !classifier.load(argv[2])) { std::cerr << "Cannot load image or cascade\n"; return 2; }
    cv::Mat gray;
    cv::cvtColor(image, gray, cv::COLOR_BGR2GRAY);
    cv::equalizeHist(gray, gray);
    std::vector<cv::Rect> faces;
    classifier.detectMultiScale(gray, faces, 1.1, 3, 0, cv::Size(30, 30));
    for (const auto& face : faces) cv::rectangle(image, face, cv::Scalar(0, 0, 255), 3);
    if (!cv::imwrite(argv[3], image)) { std::cerr << "Cannot save result\n"; return 2; }
    std::cout << "Detected faces: " << faces.size() << '\n';
    if (argc > 4 && std::string(argv[4]) == "--show") { cv::imshow("Detected faces", image); cv::waitKey(0); }
    return 0;
}
