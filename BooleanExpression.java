public class BooleanExpression {
    private final Expression left;
    private final Expression right;
    private final Operator operator;

    public BooleanExpression(Expression left, Operator operator, Expression right) {
        this.left = left;
        this.operator = operator;
        this.right  = right;
    }

    public Expression getLeft() {
        return left;
    }

    public Expression getRight() {
        return right;
    }

    public Operator getOperator() {
        return operator;
    }

    public String toString() {
        return String.format("(%s) %s (%s)", left, operator, right);
    }
}