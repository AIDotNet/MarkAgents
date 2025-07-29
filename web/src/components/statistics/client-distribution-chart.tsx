"use client"

import * as React from "react"
import { PieChart, Pie, Cell, ResponsiveContainer, Tooltip, Legend } from "recharts"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"

interface ClientDistribution {
  clientName: string
  connectionCount: number
  percentage: number
  color: string
}

interface ClientDistributionChartProps {
  data: ClientDistribution[]
  loading?: boolean
}

const RADIAN = Math.PI / 180

const renderCustomizedLabel = (entry: any) => {
  const radius = entry.innerRadius + (entry.outerRadius - entry.innerRadius) * 0.5
  const x = entry.cx + radius * Math.cos(-entry.midAngle * RADIAN)
  const y = entry.cy + radius * Math.sin(-entry.midAngle * RADIAN)

  return (
    <text 
      x={x} 
      y={y} 
      fill="white" 
      textAnchor={x > entry.cx ? 'start' : 'end'} 
      dominantBaseline="central"
      fontSize="12"
      fontWeight="bold"
    >
      {entry.percentage > 5 ? `${entry.percentage.toFixed(1)}%` : ''}
    </text>
  )
}

export function ClientDistributionChart({ data, loading = false }: ClientDistributionChartProps) {
  if (loading) {
    return (
      <Card className="animate-pulse">
        <CardHeader>
          <div className="h-6 bg-gray-200 rounded w-1/3"></div>
          <div className="h-4 bg-gray-200 rounded w-1/2 mt-2"></div>
        </CardHeader>
        <CardContent>
          <div className="h-[400px] bg-gray-200 rounded"></div>
        </CardContent>
      </Card>
    )
  }

  if (!data.length) {
    return (
      <Card>
        <CardHeader>
          <CardTitle>客户端连接分布</CardTitle>
          <CardDescription>各客户端连接情况占比分布</CardDescription>
        </CardHeader>
        <CardContent>
          <div className="flex items-center justify-center h-[400px] text-muted-foreground">
            暂无数据
          </div>
        </CardContent>
      </Card>
    )
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle>客户端连接分布</CardTitle>
        <CardDescription>各客户端连接情况占比分布</CardDescription>
      </CardHeader>
      <CardContent>
        <div className="h-[400px] w-full">
          <ResponsiveContainer width="100%" height="100%">
            <PieChart>
              <Pie
                data={data}
                cx="50%"
                cy="50%"
                labelLine={false}
                label={renderCustomizedLabel}
                outerRadius={120}
                fill="#8884d8"
                dataKey="connectionCount"
              >
                {data.map((entry, index) => (
                  <Cell key={`cell-${index}`} fill={entry.color} />
                ))}
              </Pie>
              <Tooltip
                content={({ active, payload }) => {
                  if (active && payload && payload.length) {
                    const data = payload[0].payload as ClientDistribution
                    return (
                      <div className="rounded-lg border bg-background p-3 shadow-sm">
                        <div className="grid grid-cols-1 gap-2">
                          <div className="flex items-center gap-2">
                            <div 
                              className="w-3 h-3 rounded-full" 
                              style={{ backgroundColor: data.color }}
                            />
                            <span className="font-medium">{data.clientName}</span>
                          </div>
                          <div className="text-sm text-muted-foreground">
                            <div>连接次数: {data.connectionCount.toLocaleString()}</div>
                            <div>占比: {data.percentage.toFixed(2)}%</div>
                          </div>
                        </div>
                      </div>
                    )
                  }
                  return null
                }}
              />
              <Legend 
                content={({ payload }) => (
                  <div className="flex flex-wrap justify-center gap-4 mt-4">
                    {payload?.map((entry, index) => (
                      <div key={index} className="flex items-center gap-2">
                        <div 
                          className="w-3 h-3 rounded-full" 
                          style={{ backgroundColor: entry.color }}
                        />
                        <span className="text-sm text-muted-foreground">
                          {entry.value}
                        </span>
                      </div>
                    ))}
                  </div>
                )}
              />
            </PieChart>
          </ResponsiveContainer>
        </div>
      </CardContent>
    </Card>
  )
}