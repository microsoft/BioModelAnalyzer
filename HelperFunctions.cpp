/*
 * HelperFunctions.cpp
 *
 *  Created on: 19 Mar 2014
 *      Author: np183
 */


#include "HelperFunctions.h"


using std::map;
using std::string;

 map<string,bool> splitConjunction(const string& initializer) {
	const string spaces{" \t\n"};
	const string spaces_or_and{spaces+'&'};
	map<string,bool> ret;

	for (size_t position{initializer.find_first_not_of(spaces_or_and)} ;
		 position != std::string::npos ;
		 position=initializer.find_first_not_of(spaces_or_and,position) ) {
		size_t end{initializer.find_first_of(spaces_or_and,position)};
		size_t length{(end==std::string::npos ? end : end-position)};
		string temp{initializer.substr(position,length)};

		bool positive=true;
		if (temp.at(0)=='!') {
			positive=false;
			temp = temp.substr(1,temp.length()-1);
		}
		else if (temp.find('=')!=std::string::npos) {
			const string error{"Not ready to support multi-value variables"};
			throw error;
		}

		ret.insert(make_pair(temp,positive));

		if (initializer.find_first_of('&',end)== std::string::npos &&
			initializer.find_first_not_of(spaces,end) != std::string::npos) {
			const string error{"Condition "+initializer+" is malformed"};
			throw error;
		}
	}

	return ret;
}
