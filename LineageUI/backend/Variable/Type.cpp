// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
#include "Type.h"


bool Type::operator!=(const Type& other) const {
	return !(this->operator==(other));
}

bool Type::Value::operator!=(const Value& other) const {
	return !(this->operator==(other));
}
