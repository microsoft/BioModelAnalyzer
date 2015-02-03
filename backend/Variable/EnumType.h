
#ifndef ENUMTYPE_H_
#define ENUMTYPE_H_

#include<string>
#include<vector>
#include<iosfwd>

#include "Type.h"

class EnumType : public Type
{
public:
	EnumType() = default;
	EnumType(const EnumType&) = delete;
	EnumType(EnumType&&) = delete;
	EnumType& operator=(const EnumType&) = delete;
	EnumType& operator=(EnumType&&) = delete;

	virtual ~EnumType() = default;

	Type::Types type() const override;

	void addElem(const std::string&);
	unsigned int size() const;
	bool isMember(const std::string&) const;

	 bool operator==(const Type& other) const override;

	class Value : public Type::Value {
	public:
		Value() =delete;
		Value(const Value&) = delete;
		Value(Value&&) = delete;
		Value& operator=(const Value&) = delete;
		Value& operator=(Value&&) = delete;
		Value(const EnumType&, const std::string&);

		virtual ~Value() = default;

		bool isValid() const;
		std::string value() const;
		const Type& type() const override;
		Type::Value* copy() const override;

		bool operator==(const Type::Value& other) const override;
		bool operator()() const override;

		std::string toString() const override;
		friend std::ostream& operator<<(std::ostream&, const Value&);
	private:
		Value(const EnumType&, const std::vector<std::string>::const_iterator&);

		const EnumType& _myEnum;
		std::vector<std::string>::const_iterator _it;
	};

private:
	std::vector<std::string> _elements;
};

#endif /* ENUMTYPE_H_ */
