// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
// BackEndClassLibrary1.h

using namespace System;
using namespace System::Runtime::InteropServices;

/*
[06 / 03 / 2015 14:41] Gavin Smyth :
http ://support.microsoft.com/kb/311259
[06 / 03 / 2015 14:41] Gavin Smyth :
https ://msdn.microsoft.com/en-us/library/d1ae6tz5.aspx
*/

namespace BackEndClassLibrary1 {

	public ref class Class1
	{
	public:
		static System::String^ foo(System::String^ x);
		static System::String^ getPrograms(System::String^ x);

	private:
		static std::vector<std::string> vecStringFromSysString(System::String^ x);
		static System::String^ sysStringFromVecString(std::vector<std::string> y);
	};
}
