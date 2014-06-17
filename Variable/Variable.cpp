
#include <iostream>

#include "Variable.h"
#include "BoolType.h"

using std::string;
using std::ostream;

Variable::Variable(const string& name, bool val) 
	: _name(name), _type(BoolType::getInstance()), _val(new BoolType::Value(val))
{
}

Variable::Variable(const string& name, const Type& t, Type::Value* v)
	: _name(name), _type(t), _val(v) 
{}

Variable::~Variable()
{
	// The type is not owned by the variable
	// The value is owned by the variable
	delete _val;
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
