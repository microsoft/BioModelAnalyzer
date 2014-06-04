#include "BoolType.h"


BoolType::BoolType()
{
}


BoolType::~BoolType()
{
}

BoolType& BoolType::getInstance() {
	static BoolType theBT;
	return theBT;
}

bool BoolType::operator==(const Type& other) const {
	return (this == &other);
}

BoolType::Value::Value(const bool val) : _val(val) {

}

