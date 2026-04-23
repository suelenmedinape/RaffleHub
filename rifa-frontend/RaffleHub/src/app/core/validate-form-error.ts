import {ZodError} from "zod";

export function extractZodErrors(error: unknown): Record<string, string> {
    if (error instanceof ZodError) {
        const validationErrors: Record<string, string> = {};
        error.issues.forEach((issue) => {
            const field = issue.path[0] as string;
            validationErrors[field] = issue.message;
        });
        return validationErrors;
    }
    return {};
}