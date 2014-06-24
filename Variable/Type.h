
#ifndef TYPE_H_
#define TYPE_H_

#include <string>

class Type
{
public:
	Type();
	virtual ~Type();

	virtual bool operator==(const Type& other) const=0;
	virtual bool operator!=(const Type& other) const;

	class Value {
	public:
		Value();
		virtual ~Value();

		virtual bool operator==(const Value&) const=0;
		virtual bool operator!=(const Value&) const;
		virtual bool operator()() const=0;
		virtual const Type& type() const=0;
		virtual Type::Value* duplicate() const = 0;

		virtual std::string toString() const=0;
	};
};

#endif