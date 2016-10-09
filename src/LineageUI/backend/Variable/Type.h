
#ifndef TYPE_H_
#define TYPE_H_

#include <string>

class Type
{
public:

	Type() = default;
	virtual ~Type() = default;

	enum Types{ BOOL, ENUM };

	virtual bool operator==(const Type& other) const=0;
	virtual bool operator!=(const Type& other) const final;

	virtual Types type() const=0;

	class Value {
	public:
		Value() = default;
		virtual ~Value() = default;
		Value(const Value&) = delete;
		Value(Value&&) = delete;
		Value& operator=(const Value&) = delete;
		Value& operator=(Value&&) = delete;

		virtual bool operator==(const Value&) const=0;
		virtual bool operator!=(const Value&) const final;
		virtual bool operator()() const=0;
		virtual const Type& type() const=0;
		virtual Type::Value* copy() const = 0;

		virtual std::string toString() const=0;
	};
};

#endif