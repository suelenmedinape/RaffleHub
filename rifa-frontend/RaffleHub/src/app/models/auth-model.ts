import z from "zod";

export interface AuthModel {
  fullName?: string;
  email: string;
  phone?: string;
  password: string;
}

export interface AuthData {
  token: string;
  fullName: string;
  phone: string;
  roles: string[];
  expiration: string;
  refreshToken: string;
}

export type RegisterDto = Required<AuthModel>;

export type LoginDto = Omit<Required<AuthModel>, 'fullName' | 'phone'>;

const passwordRegex = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@#$%^&+=!*()_\-{}[\]:;"'<>,.?/\\|`~])/;

export const LoginSchema = z.object({
    email: z.string().email('Entre com um email válido').min(1, 'Email é obrigatório'),
    password: z.string().min(1, 'Senha é obrigatória')
});

export type LoginSchema = z.infer<typeof LoginSchema>;

const phoneRegex = /^\(?\d{2}\)?\s?\d{4,5}-?\d{4}$/;

export const RegisterSchema = z.object({
    fullName: z.string().min(1, 'Nome é obrigatório'),
    email: z.string().email('Entre com um email válido').min(1, 'Email é obrigatório'),
    phone: z.string()
        .min(1, 'Telefone é obrigatório')
        .regex(phoneRegex, 'Telefone deve conter DDD (XX) e estar em um formato válido')
        .transform(phone => {
            const cleaned = phone.replace(/\D/g, '');
            
            const withoutCountry = cleaned.replace(/^55/, '');
            
            const ddd = withoutCountry.slice(0, 2);
            const firstPart = withoutCountry.slice(2, -4);
            const lastPart = withoutCountry.slice(-4);
            
            return `${ddd}${firstPart}${lastPart}`;
        }),
    password: z.string()
        .min(6, 'Senha deve ter no mínimo 6 caracteres')
        .regex(passwordRegex, 'Senha deve conter: letra maiúscula, minúscula, número e caractere especial')
});

export type RegisterSchema = z.infer<typeof RegisterSchema>;


