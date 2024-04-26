// OpenCVWrapper.h

#include "opencv2\opencv.hpp"
#include <msclr\marshal_cppstd.h>
#pragma once

using namespace System;
using namespace cv;

namespace OpenCVWrapper {

	public ref class ProcessImage
	{
	public:
		void CreateImage(System::String^ imagePath, UInt32 rows, UInt32 columns, byte type, ULONG step, void* data)
		{
			string path = msclr::interop::marshal_as<std::string>(imagePath);

			//Create Mat object from data buffer
			Mat image = Mat(rows, columns, type, data, step);
			//memcpy(image.data, data, columns * rows);

			//Create image from mat object
			cv::imwrite(path, image);

		}		
	};
}
