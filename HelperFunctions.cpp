/*
 * HelperFunctions.cpp
 *
 *  Created on: 19 Mar 2014
 *      Author: np183
 */


#include "HelperFunctions.h"


using std::map;
using std::vector;
using std::string;

vector<string> splitOn(char c, const string& line) {
	vector<string> ret{};
	size_t current{0};
	size_t next{0};
	do {
		next=line.find_first_of(c,current);
		ret.push_back(line.substr(current,next-current));
		current = next+1;
	}  while (next != std::string::npos);
	return ret;
}

string removeSpace(const string& in) {
	if (in.size()==0) {
		return in;
	}

	const string spaces{" \t\n"};
	size_t start{in.find_first_not_of(spaces)};
	size_t end{in.find_last_not_of(spaces)};
	return in.substr(start,end-start+1);
}

map<string,bool> splitConjunction(const string& initializer) {
	vector<string> fields{splitOn('&',initializer)};

	map<string,bool> ret{};

	for (string field : fields) {
		field = removeSpace(field);
		if (field.size()==0) {
			const string error{"Empty conjunct!"};
			throw error;
		}

		bool positive=true;
		if (field.at(0)=='!') {
			positive=false;
			field = field.substr(1,field.length()-1);
		}
		else if (field.find('=')!=std::string::npos) {
			const string error{"Not ready to support multi-value variables"};
			throw error;
		}

		ret.insert(make_pair(field,positive));
	}

	return ret;
}
