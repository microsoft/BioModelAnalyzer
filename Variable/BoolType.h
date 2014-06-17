
#ifndef BOOLTYPE_H_
#define BOOLTYPE_H_


#include "Type.h"

class BoolType :
	public Type
{
public:
	BoolType(const BoolType&) = delete;
	BoolType& operator=(const BoolType&) = delete;

	virtual ~BoolType();

	virtual bool operator==(const Type& other) const;

	static BoolType& getInstance();

	class Value : public Type::Value {
	public:
		Value() = delete;
		Value(const bool);

		virtual ~Value();

		bool value() const;
		virtual bool operator==(const Type::Value& other) const;
		virtual const Type& type() const = 0;

		virtual std::string toString() const;
	private:
		bool _val;
	};

private:
	BoolType();
};

#endif 