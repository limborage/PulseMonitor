export interface Metric {
  deviceId: string;
  cpuUsage: number;
  memoryUsage: number;
  timestamp: number;
}

export const DEFAULT_METRIC: Metric = {
  deviceId: 'N/A',
  cpuUsage: 0,
  memoryUsage: 0,
  timestamp: 0,
};
