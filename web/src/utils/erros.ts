import type { FieldValues, Path, UseFormSetError } from 'react-hook-form';
import { problema } from '../api/client.ts';

/**
 * Distribui os erros 400 da API nos campos pelo nome (campo "errors" do ProblemDetails — spec §5.5).
 * Devolve true se algum erro foi colocado em um campo.
 */
export function aplicarErrosDaApi<T extends FieldValues>(erro: unknown, setError: UseFormSetError<T>, campos: string[]): boolean {
  const p = problema(erro);
  if (p?.status !== 400 || !p.errors) return false;

  let aplicou = false;
  for (const [campo, mensagens] of Object.entries(p.errors)) {
    if (campos.includes(campo)) {
      setError(campo as Path<T>, { type: 'api', message: mensagens[0] }, { shouldFocus: !aplicou });
      aplicou = true;
    }
  }
  return aplicou;
}
