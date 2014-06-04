#include "Type.h"


Type::Type()
{
}


Type::~Type()
{
}

bool Type::operator!=(const Type& other) const {
	return !(this->operator==(other));
}

Type::Value::Value() {
}

Type::Value::~Value() {
}

bool Type::Value::operator!=(const Value& other) const {
	return !(this->operator==(other));
}