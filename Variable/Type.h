
#ifndef TYPE_H_
#define TYPE_H_

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
	};
};

#endif