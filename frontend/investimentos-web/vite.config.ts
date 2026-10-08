import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig(({ command }) => ({
    plugins: [react()],

    /*
     * O servidor local deve ser independente do Azure.
     * No npm run dev, qualquer VITE_API_URL herdada da maquina
     * e sobrescrita para a API local da solution.
     * O build de producao continua recebendo VITE_API_URL pelo pipeline.
     */
    define: command === 'serve'
        ? {
            'import.meta.env.VITE_API_URL': JSON.stringify('https://localhost:7237'),
        }
        : undefined,

    server: {
        watch: {
            ignored: ['**/.vs/**'],
        },
    },
}))
