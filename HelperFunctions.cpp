/*
 * HelperFunctions.cpp
 *
 *  Created on: 19 Mar 2014
 *      Author: np183
 */


#include "HelperFunctions.h"
#include "Expression\AndExp.h"
#include "Expression\BoolVar.h"
#include "Expression\EqExp.h"
#include "Expression\Negation.h"
#include "Expression\NeqExp.h"


using std::map;
using std::vector;
using std::string;
using std::make_pair;

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

BoolExp* parseBoolExp(const string& boolexp) {
	vector<string> fields{ splitOn('&', boolexp) };

	if (fields.size() > 1) {
		BoolExp* prev{ nullptr };
		BoolExp* temp{ nullptr };
		for (string field : fields) {
			field = removeSpace(field);
			if (field.size() == 0) {
				const string error{ "Empty conjunct!" };
				if (prev) {
					delete prev;
				}
				throw error;
			}
			try {
				temp = parseSimpleBoolExp(field);
			}
			catch (const string& err) {
				if (prev) {
					delete prev;
				}
				throw err;
			}
			if (prev) {
				prev = new AndExp(prev, temp);
			}
			else {
				prev = temp;
			}
			temp = nullptr;
		}

		return prev;
	}
	else {
		return parseSimpleBoolExp(removeSpace(fields[0]));
	}
}

BoolExp* parseSimpleBoolExp(const string& exp) {
	if (exp.find("&") != std::string::npos) {
		const string error{ " The symbol & should not appear here!" };
		throw error;
	}
	if (exp.at(0) == '!') {
		if (exp.find('=') != std::string::npos) {
			const string error{ "Expression of the form !var=val is not allowed. Change to var!=val!" };
			throw error;
		}
		return new Negation(new BoolVar(exp.substr(1, exp.length() - 1)))
	}
	else if (exp.find("!=") != std::string::npos) {
		string varname = exp.substr(0, exp.find("!="));
		string value = exp.substr(exp.find("!=") + 2, exp.length() - exp.find("!=") - 2);
		return new NeqExp(varname, value);
	}
	else if (exp.find('=') != std::string::npos) {
		string varname = exp.substr(0, exp.find("="));
		string value = exp.substr(exp.find("=") + 1, exp.length() - exp.find("=") - 1);
		return new EqExp(varname, value);
	}
	else {
		return new BoolVar(exp);
	}
}


map<string,Variable*> splitConjunction(const string& initializer) {
	vector<string> fields{splitOn('&',initializer)};

	map<string,Variable*> ret{};

	for (string field : fields) {
		field = removeSpace(field);
		if (field.size()==0) {
			const string error{"Empty conjunct!"};
			throw error;
		}

		bool positive=true;
		if (field.at(0)=='!') {
			if (field.find('=') != std::string::npos) {
				const string error{ "Expression of the form !var=val is not allowed. Change to var!=val!" };
				throw error;
			}
			positive=false;
			field = field.substr(1,field.length()-1);
		}
		//else if (field.find("!=") != std::string::npos) {
		//	string varname = field.substr(0, field.find("!="));
		//	string value = field.substr(field.find("!=") + 2, field.length() - field.find("!=") - 2);
		//	ret.insert(make_pair(varname, new Variable()))
		//}
		else if (field.find('=')!=std::string::npos) {
			const string error{"Not ready to support multi-value variables"};
			throw error;
		}

		ret.insert(make_pair(field,new Variable(field,positive)));
	}

	return ret;
}
