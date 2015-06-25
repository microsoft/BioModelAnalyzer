class TestOperand implements BMA.Operators.IOperand {

    public test: string;

    public GetFormula() {
        return this.test;
    }

}

describe('Keyframes',() => {

    //var first = 'First';
    //var kfrm1 = new BMA.Operators.Keyframe(first);
    //var kfrm2 = new BMA.Operators.Keyframe('Second');

    //var opname = 'Always';
    //var opfun = function () {
    //    return opname;
    //}

    //var op1 = new BMA.Operators.Operator(opname, opfun);
    ////var ond1 = new BMA.Operators.Keyframe(kfrm1);
    ////var ond2 = new BMA.Operators.Operand(kfrm2);

    //var op = new BMA.Operators.Operation();
    //op.Operator = op1;
    //op.Operands = [kfrm1, kfrm2];

    var formulacreator = function (funcname): BMA.Operators.IGetFormula {
        return function (op: BMA.Operators.IOperand[]) {
            var f = '(' + funcname;
            for (var i = 0; i < op.length; i++) {
                f += ' ' + op[i].GetFormula();
            }
            return f + ')';
        }
    }

    it ("creates keyframe with name",() => {
        var k1 = new BMA.Operators.Keyframe('one');
        expect(k1.GetFormula()).toEqual('one');
    });
        
    it('creates operator with name and GetFormula()',() => {
        var op = new BMA.Operators.Operator('name', function (op: BMA.Operators.IOperand[]) {
            var f = '';
            for (var i = 0; i < op.length; i++)
                f += op[i].GetFormula();
            return f;
        });

        var k1 = new TestOperand();
        k1.test = 'test1';
        expect(op.GetFormula([k1])).toEqual('test1');

        var k2 = new TestOperand();
        k2.test = 'test2';
        expect(op.GetFormula([k1, k2])).toEqual('test1test2');
    });

    it('creates operator for BMA',() => {
        

        var op1 = new BMA.Operators.Operator('Until', formulacreator('Until'));

        var k1 = new TestOperand();
        k1.test = 'test1';
        expect(op1.GetFormula([k1])).toEqual('(Until test1)');

        var k2 = new TestOperand();
        k2.test = 'test2';
        expect(op1.GetFormula([k1, k2])).toEqual('(Until test1 test2)');
    });

    it('creates operation with 2 keyframes',() => {
        var k1 = new BMA.Operators.Keyframe('one');
        var k2 = new BMA.Operators.Keyframe('two');
        var k3 = new BMA.Operators.Keyframe('three');

        var op1 = new BMA.Operators.Operator('Until', formulacreator('Until'));
        var op2 = new BMA.Operators.Operator('Always', formulacreator('Always'));

        var op = new BMA.Operators.Operation();
        op.Operands = [k1, k2];
        op.Operator = op1;

        var opp = new BMA.Operators.Operation(); 
        opp.Operands = [k3, op];
        opp.Operator = op2;

        expect(op.GetFormula()).toEqual('(Until one two)');
        expect(opp.GetFormula()).toEqual('(Always three (Until one two))');
    });

    it('creates tree of operations',() => {
        var k1 = new BMA.Operators.Keyframe('one');
        var k2 = new BMA.Operators.Keyframe('two');
        var k3 = new BMA.Operators.Keyframe('three');
        var k4 = new BMA.Operators.Keyframe('four');

        var op1 = new BMA.Operators.Operator('And', formulacreator('And'));
        var op2 = new BMA.Operators.Operator('Or', formulacreator('Or'));

        var oper1 = new BMA.Operators.Operation();
        oper1.Operands = [k1, k2];
        oper1.Operator = op1;

        var oper2 = new BMA.Operators.Operation();
        oper2.Operands = [k3, k4];
        oper2.Operator = op2;

        var oper = new BMA.Operators.Operation(); 
        oper.Operands = [oper2, oper1];
        oper.Operator = op2;

        expect(oper.GetFormula()).toEqual('(Or (Or three four) (And one two))');

        var not = new BMA.Operators.Operation();
        not.Operands = [oper];
        not.Operator = new BMA.Operators.Operator('Not', formulacreator('Not'));;

        expect(not.GetFormula()).toEqual('(Not (Or (Or three four) (And one two)))');
    });


}); 