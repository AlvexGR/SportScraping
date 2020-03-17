export class ApiResult<T> {
  succeed: boolean;
  error: string;
  result: T;
  executionTime: number;
}
