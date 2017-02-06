// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
#include "EnumType.h"

#include <iostream>
#include <typeinfo>

using std::string;
using std::vector;
using std::ostream;

Type::Types EnumType::type() const {
	return Type::Types::ENUM;
}


void EnumType::addElem(const string& e) {
	_elements.push_back(e);
}

unsigned int EnumType::size() const {
	return _elements.size();
}

bool EnumType::isMember(const string& e) const {
	for (auto elem : _elements) {
		if (elem == e) {
			return true;
		}
	}
	return false;
}

bool EnumType::operator==(const Type& other) const {
	if (typeid(*this) != typeid(other)) {
		return false;
	}

	const EnumType* eOther{ dynamic_cast<const EnumType*>(&other) };
	if (_elements.size() != eOther->_elements.size()) {
		return false;
	}
	auto myIt = _elements.begin();
	auto otherIt = eOther->_elements.begin();
	while (myIt != _elements.end()) {
		if ((*myIt) != (*otherIt)) {
			return false;
		}
		++myIt;
		++otherIt;
	}
	return true;
}

EnumType::Value::Value(const EnumType& en, const string& val)
	: _myEnum(en)
{
	for (auto elemIt = _myEnum._elements.begin(); elemIt != _myEnum._elements.end(); ++elemIt) {
		if ((*elemIt) == val) {
			_it = elemIt;
			return;
		}
	}
	_it = _myEnum._elements.end();
}

EnumType::Value::Value(const EnumType& en, const std::vector < std::string > ::const_iterator& it)
	: _myEnum(en), _it(it)
{
}


string EnumType::Value::value() const {
	if (_it == _myEnum._elements.end()) {
		return "";
	}
	return *_it;
}

bool EnumType::Value::isValid() const {
	return _it != _myEnum._elements.end();
}

const Type& EnumType::Value::type() const {
	return _myEnum;
}

Type::Value* EnumType::Value::copy() const {
	return new EnumType::Value(_myEnum, _it);
}


bool EnumType::Value::operator==(const Type::Value& other) const {
	if (typeid(*this) != typeid(other)) {
		return false;
	}
//	const Value& eOther{ dynamic_cast<const Value&>(other) };
	const Value *eOther ( dynamic_cast<const Value*>(&other) );
	if (this->_myEnum != eOther->_myEnum) {
		return false;
	}
	return this->_it == eOther->_it;
}

bool EnumType::Value::operator()() const {
	return false;
}

ostream& operator<<(ostream& out, const EnumType::Value& val) {
	if (!val.isValid()) {
		out << "";
	}
	else {
		out << *(val._it);
	}
	return out;
}

string EnumType::Value::toString() const {
	return *_it;
}
