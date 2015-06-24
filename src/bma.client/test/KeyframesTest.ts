describe('Keyframes',() => {

    var first = 'First';
    var kfrm1 = new BMA.Operators.Keyframe(first);
    var kfrm2 = new BMA.Operators.Keyframe('Second');

    var opname = 'Always';
    var opfun = function () {
        return opname;
    }

    var op1 = new BMA.Operators.Operator(opname, opfun);
    var ond1 = new BMA.Operators.Operand(kfrm1);
    var ond2 = new BMA.Operators.Operand(kfrm2);

    var op = new BMA.Operators.Operation();
    op.Operator = op1;
    op.Operands = [ond1, ond2];

    describe("GetFormula()",() => {
        console.log(op.GetFormula());
    });
        
}); 