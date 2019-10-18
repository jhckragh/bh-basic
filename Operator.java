public enum Operator {
    LT, EQ, GT, PLUS, MINUS, TIMES, DIV;

    public String toString() {
        switch (this) {
            case LT: return "<";
            case EQ: return "=";
            case GT: return ">";
            case PLUS: return "+";
            case MINUS: return "-";
            case TIMES: return "*";
            case DIV: return "/";
            default: throw new RuntimeException("internal error");
        }
    }
}
