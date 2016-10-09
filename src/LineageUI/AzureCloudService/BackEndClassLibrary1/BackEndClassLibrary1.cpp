// This is the main DLL file.

#include <string>
#include <vector>
#include <msclr/marshal_cppstd.h>
#include "BackEndClassLibrary1.h"
#include "../../backend/LineageLib.h"

using std::string;
using std::vector;

System::String^ BackEndClassLibrary1::Class1::foo(System::String^ x)
{
	vector<string> program = vecStringFromSysString(x);
	vector<string> res = simulate(program, "blah");
	res.push_back("Error");
	String ^ y = gcnew String(res[0].c_str());
	String ^ z = y + ":" + x;
	return z;
	//return x->Length(); 

}

System::String^ BackEndClassLibrary1::Class1::getPrograms(System::String^ x)
{
	vector<string> program = vecStringFromSysString(x);
	vector<string> cells = programs(program);
	String^ res = sysStringFromVecString(cells);
	return res;
}

vector<string> BackEndClassLibrary1::Class1::vecStringFromSysString(System::String^ x) {
	vector<string> ret;
	string xstr = msclr::interop::marshal_as<string>(x);
	string::size_type start = 0;
	for (string::size_type end = xstr.find('\n'); end != string::npos; start = end + 1, end = xstr.find('\n', start)) {
		ret.push_back(xstr.substr(start, end - start));
	}
	ret.push_back(msclr::interop::marshal_as<std::string>(x));
	return ret;
}

System::String^ BackEndClassLibrary1::Class1::sysStringFromVecString(vector<string> y) {
	String^ res;
	bool first = true;
	for (auto val : y) {
		if (!first) {
			res = res + "::";
		}
		else {
			first = false;
		}
		res = res + gcnew String(val.c_str());
	}
	return res;
}