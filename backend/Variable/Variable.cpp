
#include <iostream>

#include "Variable.h"
#include "BoolType.h"

using std::string;
using std::ostream;

Variable::Variable(const Variable& other)
	: _name(other._name), _type(other._type), _val(other._val->copy())
{
}

Variable::Variable(Variable&& other) 
	: _name(std::move(other._name)), _type(other._type), _val(other._val)
{
	other._val = nullptr;
}

Variable::Variable(const string& name, bool val) 
	: _name(name), _type(BoolType::getInstance()), _val(new BoolType::Value(val))
{
}

Variable::Variable(const string& name/*, const Type& t*/, Type::Value* v)
	: _name(name), _type(v->type()), _val(v) 
{
	//if (t != v->type()) {
	//	string error{ "Variable " };
	//	error += name;
	//	error += " initalized with value "; 
	//	error += v->toString();
	//	error += " that does not match its type";
	//	throw error;
	//}
}

Variable::~Variable()
{
	// The type is not owned by the variable
	// The value is owned by the variable
	if (_val) {
		delete _val;
	}
}

void Variable::set(bool val) {
	if (_type != BoolType::getInstance()) {
		string error{ "Assigning a Boolean value to non Boolean variable " };
		error += _name;
		throw error;
	}
	if (_val) {
		delete _val;
	}
	_val=new BoolType::Value(val);
}

void Variable::set(const Type::Value& val) {
	if (_type != val.type()) {
		string error{ "Assigning value " };
		error += val.toString();
		error += " to variable ";
		error += _name;
		throw error;
	}
	if (_val) {
		delete _val;
	}
	_val = val.copy();
}

const Type::Value* Variable::value() const {
	return _val;
}

bool Variable::operator==(const Variable& other) const {
	return _type == other._type && _val == other._val;
}

bool Variable::operator!=(const Variable& other) const {
	return !this->operator==(other);
}

ostream& operator<< (ostream& out, const Variable& var) {
	if (var._type.operator==(BoolType::getInstance())) {
		const BoolType::Value& bVal(dynamic_cast<const BoolType::Value&>(*var._val));
		if (!bVal.value()) {
			out << "!";
		}
		out << var._name;
	}
	else {
		out << var._name;
		out << "=";
		out << var._val->toString();
	}
	return out;
}
