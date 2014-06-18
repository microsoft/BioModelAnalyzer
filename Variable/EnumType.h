
#ifndef ENUMTYPE_H_
#define ENUMTYPE_H_

#include<string>
#include<vector>
#include<iosfwd>

#include "Type.h"

class EnumType :
	public Type
{
public:
	EnumType();
	virtual ~EnumType();

	void addElem(const std::string&);
	unsigned int size() const;
	bool isMember(const std::string&) const;

	virtual bool operator==(const Type& other) const;

	class Value : public Type::Value {
	public:
		Value() =delete;
		Value(const EnumType&, const std::string&);

		virtual ~Value();
		
		bool isValid() const;
		std::string value() const;
		virtual const Type& type() const;
		virtual Type::Value* duplicate() const;

		virtual bool operator==(const Type::Value& other) const;

		virtual std::string toString() const;
		friend std::ostream& operator<<(std::ostream&, const Value&);
	private:
		const EnumType& _myEnum;
		std::vector<std::string>::const_iterator _it;
	};

private:
	std::vector<std::string> _elements;
};

#endif /* TYPE_H_ */