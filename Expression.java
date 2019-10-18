public abstract class Expression {
    public static class Binary extends Expression {
        private final Expression left;
        private final Expression right;
        private final Operator operator;

        public Binary(Expression left, Operator operator, Expression right) {
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

    public static class Identifier extends Expression {
        private final String variableName;

        public Identifier(String variableName) {
            this.variableName = variableName;
        }

        public String getVariableName() {
            return variableName;
        }

        public String toString() {
            return variableName;
        }
    }

    public static class Literal extends Expression {
        private final int value;

        public Literal(int value) {
            this.value = value;
        }

        public int getValue() {
            return value;
        }

        public String toString() {
            return Integer.toString(value);
        }
    }
}