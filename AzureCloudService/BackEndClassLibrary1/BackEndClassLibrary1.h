// BackEndClassLibrary1.h

#pragma once

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
		// TODO: Add your methods for this class here.
		public : 
		static System::String^ foo(System::String^ x)
		{
			String ^ y = gcnew String("ReturnFromC++BackEnd:"); 
			String ^ z = y + ":" + x;
			return z; 
			//return x->Length(); 
			
		}
	};
}
