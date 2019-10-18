import java.util.*;

public abstract class Command {
    private final int lineNumber;

    public Command(int lineNumber) {
        this.lineNumber = lineNumber;
    }

    public int getLineNumber() {
        return lineNumber;
    }

    public static final class Let extends Command {
        private final String lhs;
        private final Expression rhs;

        public Let(int lineNumber, String lhs, Expression rhs) {
            super(lineNumber);
            this.lhs = lhs;
            this.rhs = rhs;
        }

        public String getLhs() {
            return lhs;
        }

        public Expression getRhs() {
            return rhs;
        }

        public String toString() {
            return String.format("%d LET[lhs=%s, rhs=%s]",
                getLineNumber(), lhs, rhs);
        }
    }

    public static final class Print extends Command {
        private final List<Object> values;

        public Print(int lineNumber, List<Object> values) {
            super(lineNumber);
            this.values = new ArrayList<>(values);
        }

        public List<Object> getValues() {
            return new ArrayList<>(values);
        }

        public String toString() {
            List<String> reprs = new ArrayList<>();
            for (Object o : values) {
                reprs.add(o.toString());
            }
            return String.format("%d PRINT[values=%s]",
                getLineNumber(), String.join(", ", reprs));
        }
    }

    public static final class Input extends Command {
        private final String prompt;
        private final List<String> variableNames;

        public Input(int lineNumber, String prompt, List<String> variableNames) {
            super(lineNumber);
            this.prompt = prompt;
            this.variableNames = new ArrayList<>(variableNames);
        }

        public String getPrompt() {
            return prompt;
        }

        public List<String> getVariableNames() {
            return new ArrayList<>(variableNames);
        }

        public String toString() {
            return String.format("%d INPUT[prompt=%s, variableNames=%s]",
                getLineNumber(), prompt, String.join(", ", variableNames));
        }
    }

    public static final class For extends Command {
        private final String variableName;
        private final Expression from;
        private final Expression to;

        public For(int lineNumber, String variableName, Expression from, Expression to) {
            super(lineNumber);
            this.variableName = variableName;
            this.from = from;
            this.to = to;
        }

        public String getVariableName() {
            return variableName;
        }

        public Expression getFrom() {
            return from;
        }

        public Expression getTo() {
            return to;
        }

        public String toString() {
            return String.format("%d FOR[var=%s, from=%s, to=%s]",
                getLineNumber(), variableName, from, to);
        }
    }

    public static final class RealizedFor extends Command {
        private final String variableName;
        private final int from;
        private final int to;

        public RealizedFor(int lineNumber, String variableName, int from, int to) {
            super(lineNumber);
            this.variableName = variableName;
            this.from = from;
            this.to = to;
        }

        public String getVariableName() {
            return variableName;
        }

        public int getFrom() {
            return from;
        }

        public int getTo() {
            return to;
        }

        public String toString() {
            return String.format("%d FOR[var=%s, from=%s, to=%s]",
                getLineNumber(), variableName, from, to);
        }
    }

    public static final class Next extends Command {
        private final String variableName;

        public Next(int lineNumber, String variableName) {
            super(lineNumber);
            this.variableName = variableName;
        }

        public String getVariableName() {
            return variableName;
        }

        public String toString() {
            return String.format("%d NEXT[var=%s]",
                getLineNumber(), variableName);
        }
    }

    public static final class If extends Command {
        private final BooleanExpression exp;
        private final Command cmd;

        public If(int lineNumber, BooleanExpression exp, Command cmd) {
            super(lineNumber);
            this.exp = exp;
            this.cmd = cmd;
        }

        public BooleanExpression getExp() {
            return exp;
        }

        public Command getCmd() {
            return cmd;
        }

        public String toString() {
            return String.format("%d IF[exp=%s, cmd=%s]",
                getLineNumber(), exp, cmd);
        }
    }

    public static final class Goto extends Command {
        private final int targetLine;

        public Goto(int lineNumber, int targetLine) {
            super(lineNumber);
            this.targetLine = targetLine;
        }

        public int getTargetLine() {
            return targetLine;
        }

        public String toString() {
            return String.format("%d GOTO[targetLine=%d]",
                getLineNumber(), targetLine);
        }
    }

    public static final class GoSub extends Command {
        private final int targetLine;

        public GoSub(int lineNumber, int targetLine) {
            super(lineNumber);
            this.targetLine = targetLine;
        }

        public int getTargetLine() {
            return targetLine;
        }

        public String toString() {
            return String.format("%d GOSUB[targetLine=%d]",
                getLineNumber(), targetLine);
        }
    }

    public static final class Return extends Command {
        public Return(int lineNumber) {
            super(lineNumber);
        }

        public String toString() {
            return getLineNumber() + " RETURN";
        }
    }

    public static final class End extends Command {
        public End(int lineNumber) {
            super(lineNumber);
        }

        public String toString() {
            return getLineNumber() + " END";
        }
    }

    public static final class Empty extends Command {
        public Empty(int lineNumber) {
            super(lineNumber);
        }

        public String toString() {
            return getLineNumber() + " EMPTY";
        }
    }
}