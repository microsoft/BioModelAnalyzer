
#ifndef BOOLTYPE_H_
#define BOOLTYPE_H_


#include "Type.h"

class BoolType :
	public Type
{
public:
	BoolType(const BoolType&) = delete;
	BoolType(BoolType&&) = delete;
	BoolType& operator=(const BoolType&) = delete;
	BoolType& operator=(BoolType&&) = delete;

	virtual ~BoolType() = default;

	bool operator==(const Type& other) const override;

	Type::Types type() const override;
	static BoolType& getInstance();

	class Value : public Type::Value {
	public:
		Value() = delete;
		Value(const Value&) = delete;
		Value(Value&&) = delete;
		Value(const bool);
		Value& operator=(const Value&) = delete;
		Value& operator=(Value&&) = delete;

		virtual ~Value() = default;

		bool value() const;
		bool operator==(const Type::Value& other) const override;
		bool operator()() const override;

		const Type& type() const override;
		Type::Value* copy() const override;

		std::string toString() const override;
	private:
		bool _val;
	};

private:
	BoolType() = default;
};

#endif 