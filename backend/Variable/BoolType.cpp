#include <typeinfo>
#include "BoolType.h"

using std::string;

BoolType& BoolType::getInstance() {
	static BoolType theBT;
	return theBT;
}

bool BoolType::operator==(const Type& other) const {
	return (this == &other);
}

BoolType::Value::Value(const bool val) : _val(val) {

}

bool BoolType::Value::value() const {
	return _val;
}

const Type& BoolType::Value::type() const {
	return BoolType::getInstance();
}

Type::Value* BoolType::Value::copy() const {
	return new Value(_val);
}

bool BoolType::Value::operator==(const Type::Value& other) const {
	if (typeid(*this) != typeid(other)) {
		return false;
	}
	const Value& bOther(dynamic_cast<const Value&>(other));
	return bOther._val == this->_val;
}

bool BoolType::Value::operator()() const {
	return _val;
}

string BoolType::Value::toString() const {
	if (_val) {
		return "TRUE"; 
	}
	return "FALSE";
}